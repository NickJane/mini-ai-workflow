/**
 * 条件节点
 * 根据JS表达式执行条件判断，支持多分支输出
 */
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';
import type { AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

/**
 * 条件规则接口
 */
export interface ConditionRule {
  /** 规则ID */
  id: string;
  /** 表达式单元（支持多种表达式类型） */
  expressionUnit: AnyExpressionUnit;
  /** 条件描述（可选） */
  description?: string;
  /** 满足条件时的输出连接ID */
  lineId?: string;
}

/**
 * 条件节点数据接口
 */
export interface ConditionNodeData extends NodeBase {
  /** 节点类型标识 */
  typeName: 'ConditionNode';
  /** 条件规则列表 */
  conditions: ConditionRule[];
  /** else分支规则 */
  elseRule: ConditionRule;
}

const typeName = 'ConditionNode';

// 条件节点模型
class ConditionNodeModel extends NodeBaseModel {
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    
    // 条件节点宽度固定
    this.width = 280;
    
    // 先初始化默认数据，再计算高度
    this.initializeNodeData();
    this.updateHeight();
  }
  
  // 初始化节点默认数据
  initializeNodeData() {
    // 生成规则ID的辅助函数
    const generateRuleId = (prefix: string) => `${prefix}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    // 设置节点默认数据（只在属性不存在时设置默认值）
    const nodeData: Partial<ConditionNodeData> = {
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "Condition",
      description: (this.properties.description as string | undefined) || "Execute different branches by condition",
      // 初始化时创建一个条件1规则
      conditions: (this.properties.conditions as ConditionRule[] | undefined) || [
        {
          id: generateRuleId('condition'),
          expressionUnit: ExpressionUnitFactory.createJSExpression(''),
          description: 'Condition 1'
        }
      ],
      // 初始化时创建else分支规则
      elseRule: (this.properties.elseRule as ConditionRule | undefined) || {
        id: generateRuleId('else'),
        expressionUnit: ExpressionUnitFactory.createJSExpression(''),
        description: 'Else branch'
      },
      defaultDescription: (this.properties.defaultDescription as string | undefined) || 'Default branch'
    };
    this.setNodeData(nodeData);
  }
  
  // 动态更新节点高度
  updateHeight() {
    const conditions = (this.properties.conditions as ConditionRule[] | undefined) || [];
    const baseHeight = 60;  // 基础高度（头部）
    const conditionHeight = 46; // 每个条件的高度
    const defaultBranchHeight = 46; // 默认分支的高度
    const padding = 20; // 内边距
    
    this.height = baseHeight + (conditions.length * conditionHeight) + defaultBranchHeight + padding;
  }
  
  /**
   * 自定义锚点
   * 左侧1个输入锚点,右侧根据条件数量+1(默认分支)动态生成锚点
   * 右侧锚点位置与HTML中显示的圆点精确对齐
   * 锚点ID使用ConditionRule的id
   */
  getDefaultAnchor() {
    const { width, height, x, y, id } = this;
    const conditions = (this.properties.conditions as ConditionRule[] | undefined) || [];
    const elseRule = this.properties.elseRule as ConditionRule | undefined;
    
    const dataArr: Array<{ x: number; y: number; id: string }> = [];
    
    // 左侧锚点：固定在垂直居中位置
    dataArr.push({
      x: x - width / 2,
      y: y,
      id: `${id}_input`
    });
    
    // 右侧锚点：与每个条件项的视觉圆点对齐
    const baseHeight = 60;  // 头部高度
    const conditionItemHeight = 46; // 每个条件项的高度(包含padding和margin)
    const itemPaddingTop = 10; // 条件项上内边距
    const anchorOffsetInItem = 6; // 锚点在条件项内容区的垂直位置(微调至视觉圆点中心)
    const nodeTopY = y - height / 2; // 节点顶部的Y坐标
    const rightX = x + width / 2; // 右边框的X坐标
    
    // 为每个条件生成锚点，锚点ID使用ConditionRule的id
    conditions.forEach((condition: ConditionRule, index: number) => {
      // 计算该条件项的Y坐标（对齐到视觉圆点的中心）
      // baseHeight是头部，然后每个条件项占conditionItemHeight
      // 圆点位于条件项顶部 + padding-top + 内容区一半高度
      const conditionY = nodeTopY + baseHeight + (index * conditionItemHeight) + itemPaddingTop + anchorOffsetInItem;
      
      dataArr.push({
        x: rightX,
        y: conditionY,
        id: condition.id  // 使用ConditionRule的id作为锚点ID
      });
    });
    
    // else分支锚点，使用elseRule的id
    if (elseRule) {
      const defaultBranchY = nodeTopY + baseHeight + (conditions.length * conditionItemHeight) + itemPaddingTop + anchorOffsetInItem;
      dataArr.push({
        x: rightX,
        y: defaultBranchY,
        id: elseRule.id  // 使用elseRule的id作为锚点ID
      });
    }
    
    return dataArr;
  }

  /**
   * 连线添加回调
   * 当从条件节点的某个锚点连接到其他节点时触发
   * @param edgeId 连接线ID
   * @param sourceAnchorId 起始锚点ID（就是ConditionRule的id，因为getDefaultAnchor中锚点ID直接使用了rule.id）
   * @param targetNodeId 目标节点ID
   * @param targetAnchorId 目标锚点ID
   */
  onEdgeAdd(edgeId: string, sourceAnchorId: string, targetNodeId: string, targetAnchorId: string): void {
    const conditions = (this.properties.conditions as ConditionRule[] | undefined) || [];
    const elseRule = this.properties.elseRule as ConditionRule | undefined;
    
    // 查找对应的条件规则（sourceAnchorId就是ruleId）
    const condition = conditions.find((c: ConditionRule) => c.id === sourceAnchorId);
    if (condition) {
      // 更新条件规则的lineId
      condition.lineId = edgeId;
      this.setNodeData({ conditions: [...conditions] });
      return;
    }
    
    // 检查是否是else分支
    if (elseRule && elseRule.id === sourceAnchorId) {
      elseRule.lineId = edgeId;
      this.setNodeData({ elseRule: { ...elseRule } });
    }
  }

  /**
   * 连线删除回调
   * 当删除从条件节点某个锚点出发的连线时触发
   * @param edgeId 连接线ID
   * @param sourceAnchorId 起始锚点ID（就是ConditionRule的id，因为getDefaultAnchor中锚点ID直接使用了rule.id）
   */
  onEdgeDelete(edgeId: string, sourceAnchorId: string): void {
    const conditions = (this.properties.conditions as ConditionRule[] | undefined) || [];
    const elseRule = this.properties.elseRule as ConditionRule | undefined;
    
    // 查找对应的条件规则（sourceAnchorId就是ruleId）
    const condition = conditions.find((c: ConditionRule) => c.id === sourceAnchorId);
    if (condition && condition.lineId === edgeId) {
      // 清除条件规则的lineId
      condition.lineId = undefined;
      this.setNodeData({ conditions: [...conditions] });
      return;
    }
    
    // 检查是否是else分支
    if (elseRule && elseRule.id === sourceAnchorId && elseRule.lineId === edgeId) {
      elseRule.lineId = undefined;
      this.setNodeData({ elseRule: { ...elseRule } });
    }
  }

  /**
   * 属性更新后的回调（重写父类方法）
   * 当条件节点的条件数量变化时，节点高度和锚点位置会改变，需要刷新所有相关连线
   * @param lf LogicFlow实例
   * @param flowStore Pinia状态管理实例
   */
  onPropertiesUpdated(lf: any, flowStore: any): void {
    if (!lf || !flowStore) return;
    
    // 先更新节点高度（根据新的条件数量）
    this.updateHeight();
    
    try {
      // 从 flowStore 获取与该节点相关的所有边
      const relatedEdges = flowStore.currentEdges.filter((edge: any) => 
        edge.sourceNodeId === this.id || edge.targetNodeId === this.id
      );
      
      if (relatedEdges.length === 0) {
        return;
      }
      
      // 使用 Pinia Store 中的边数据（已包含完整的锚点信息）
      const edgeInfos = relatedEdges.map((edge: any) => ({
        id: edge.id,  // 保留原始ID
        type: edge.type || 'polyline',
        sourceNodeId: edge.sourceNodeId,
        targetNodeId: edge.targetNodeId,
        sourceAnchorId: edge.sourceAnchorId,  // Pinia Store 中已保存
        targetAnchorId: edge.targetAnchorId,  // Pinia Store 中已保存
        properties: edge.properties
      }));
      
      // 删除旧边
      edgeInfos.forEach((edgeInfo: any) => {
        // 这会触发 flowDeigner.vue中的 edge:delete 事件, 从而同步状态管理
        lf.deleteEdge(edgeInfo.id);
      });
      
      // 获取当前节点的条件列表和else规则（可能已经发生变化）
      const currentConditions = (this.properties.conditions as ConditionRule[] | undefined) || [];
      const currentElseRule = this.properties.elseRule as ConditionRule | undefined;
      
      // 重新创建边（使用原始ID，LogicFlow会根据新的锚点位置重新连接）
      edgeInfos.forEach((edgeInfo: any) => {
        // 如果这条边是从当前节点出发的，需要检查锚点对应的条件是否还存在
        if (edgeInfo.sourceNodeId === this.id) {
          const anchorId = edgeInfo.sourceAnchorId;
          
          // 检查条件列表中是否存在该锚点对应的条件
          const conditionExists = currentConditions.some((c: ConditionRule) => c.id === anchorId);
          
          // 检查是否是else分支
          const isElseRule = currentElseRule && currentElseRule.id === anchorId;
          
          // 如果条件已经不存在了（被删除），则不重建这条边
          if (!conditionExists && !isElseRule) {
            console.log(`条件已删除，跳过重建连线: ${edgeInfo.id}`);
            return;
          }
        }
        
        // 这会触发 flowDeigner.vue中的 edge:add 事件, 从而同步状态管理
        lf.addEdge({
          id: edgeInfo.id,  // 使用原始ID，保持ID不变
          type: edgeInfo.type,
          sourceNodeId: edgeInfo.sourceNodeId,
          targetNodeId: edgeInfo.targetNodeId,
          sourceAnchorId: edgeInfo.sourceAnchorId,
          targetAnchorId: edgeInfo.targetAnchorId,
          properties: edgeInfo.properties
        });
      });
      
      console.log(`条件节点 ${this.id} 已刷新 ${relatedEdges.length} 条连线`);
    } catch (error) {
      console.error('条件节点刷新连线失败:', error);
    }
  }
}

// 条件节点视图
class ConditionNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as ConditionNodeData;
    
    // 构建条件列表显示，每个条件右侧有连接点
    const conditionsHtml = nodeData.conditions
      .map((condition: ConditionRule, index: number) => {
        // 获取表达式代码（根据 isFunctionMode 选择对应字段）
        const expressionUnit = condition.expressionUnit as any;
        const code = expressionUnit?.isFunctionMode 
          ? (expressionUnit?.functionCode || '') 
          : (expressionUnit?.expressionCode || '');
        const expression = code || 'No expression set';
        const description = condition.description || `Condition ${index + 1}`;
        const shortExpression = expression.length > 20 
          ? expression.substring(0, 20) + '...' 
          : expression;
          
        return `
          <li class="condition-item" title="${expression}">
            <div class="condition-text">
              <div class="condition-label">${description}</div>
              <div class="condition-expression">${shortExpression}</div>
            </div>
            <div class="condition-anchor">
              <div class="anchor-point"></div>
            </div>
          </li>
        `;
      })
      .join('');

    // 渲染节点HTML，参考StartNode的简洁风格
    rootEl.innerHTML = `
      <style>
        .condition-node .conditions-list {
          list-style: none;
          margin: 0;
          padding: 0;
          width: 100%;
        }
        
        .condition-node .condition-item {
          display: flex;
          align-items: center;
          justify-content: space-between;
          width: 100%;
          padding: 10px 12px;
          margin-bottom: 6px;
          background: #f8fafc;
          border: 1px solid #e2e8f0;
          border-radius: 6px;
          position: relative;
          box-sizing: border-box;
        }
        
        .condition-node .condition-item:last-child {
          margin-bottom: 0;
        }
        
        .condition-node .condition-item.default-branch {
          background: #fef2f2;
          border-color: #fecaca;
        }
        
        .condition-node .condition-text {
          flex: 1;
          text-align: left;
          display: flex;
          align-items: center;
          gap: 8px;
        }
        
        .condition-node .condition-label {
          font-weight: 600;
          font-size: 12px;
          color: #374151;
          flex-shrink: 0;
        }
        
        .condition-node .condition-expression {
          font-size: 11px;
          color: #6b7280;
          flex: 1;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }
        
        .condition-node .default-branch .condition-label {
          color: #991b1b;
        }
        
        .condition-node .default-branch .condition-expression {
          color: #dc2626;
        }
        
        .condition-node .condition-anchor {
          flex-shrink: 0;
          margin-left: 12px;
        }
        
        .condition-node .anchor-point {
          width: 14px;
          height: 14px;
          background: #3b82f6;
          border: 2px solid white;
          border-radius: 50%;
          cursor: pointer;
          box-shadow: 0 2px 4px rgba(0,0,0,0.2);
          transition: all 0.2s ease;
        }
        
        .condition-node .anchor-point:hover {
          transform: scale(1.1);
          box-shadow: 0 3px 6px rgba(0,0,0,0.3);
        }
        
        .condition-node .default-branch .anchor-point {
          background: #ef4444;
        }
      </style>
      <div class="dify-node condition-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="m679.872 348.8-301.76 188.608a127.808 127.808 0 0 1 5.12 52.16l279.936 104.96a128 128 0 1 1-22.464 59.904l-279.872-104.96a128 128 0 1 1-16.64-166.272l301.696-188.608a128 128 0 1 1 33.92 54.272z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'Condition'}</span>
        </div>
        <div class="node-info">
          <ul class="conditions-list">
            ${conditionsHtml}
            <li class="condition-item default-branch">
              <div class="condition-text">
                <div class="condition-label">ELSE</div>
                <div class="condition-expression">${nodeData.defaultDescription || 'Default branch'}</div>
              </div>
              <div class="condition-anchor">
                <div class="anchor-point"></div>
              </div>
            </li>
          </ul>
        </div>
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: ConditionNodeView,
  model: ConditionNodeModel,
};
