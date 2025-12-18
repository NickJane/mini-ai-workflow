/**
 * 变量赋值节点
 * 用于给会话变量赋值，支持JS表达式计算
 */
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';
import type { AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

/**
 * 赋值项接口
 * 每一项代表一个变量赋值操作
 */
export interface AssignmentItem {
  /** 赋值项ID */
  id: string;
  /** 目标变量名（会话变量的名称） */
  targetVariableName: string;
  /** 表达式单元（支持多种表达式类型） */
  expressionUnit: AnyExpressionUnit;
}

/**
 * 变量赋值节点数据接口
 */
export interface AssignVariableNodeData extends NodeBase {
  /** 节点类型标识 */
  typeName: 'AssignVariableNode';
  /** 赋值项列表 */
  assignments: AssignmentItem[];
}

const typeName = 'AssignVariableNode';

// 变量赋值节点模型
class AssignVariableNodeModel extends NodeBaseModel {
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    
    // 节点宽度固定
    this.width = 260;
    
    // 先初始化默认数据，再计算高度
    this.initializeNodeData();
    this.updateHeight();
  }
  
  // 初始化节点默认数据
  initializeNodeData() {
    // 生成赋值项ID的辅助函数
    const generateAssignmentId = () => `assign_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    // 设置节点默认数据（只在属性不存在时设置默认值）
    const nodeData: Partial<AssignVariableNodeData> = {
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "Assign",
      description: (this.properties.description as string | undefined) || "Assign session variables",
      // 初始化时创建一个空的赋值项
      assignments: (this.properties.assignments as AssignmentItem[] | undefined) || [
        {
          id: generateAssignmentId(),
          targetVariableName: '',
          expressionUnit: ExpressionUnitFactory.createJSExpression('')
        }
      ]
    };
    this.setNodeData(nodeData);
  }
  
  // 动态更新节点高度
  updateHeight() {
    const assignments = (this.properties.assignments as AssignmentItem[] | undefined) || [];
    const baseHeight = 60;  // 基础高度（头部）
    const assignmentHeight = 40; // 每个赋值项的高度
    const padding = 20; // 内边距
    
    this.height = baseHeight + (assignments.length * assignmentHeight) + padding;
  }

  /**
   * 属性更新后的回调（重写父类方法）
   * 当赋值项数量变化时，需要更新节点高度
   * @param lf LogicFlow实例
   * @param flowStore Pinia状态管理实例
   */
  onPropertiesUpdated(lf: any, flowStore: any): void {
    if (!lf || !flowStore) return;
    
    // 更新节点高度（根据新的赋值项数量）
    this.updateHeight();
  }
}

// 变量赋值节点视图
class AssignVariableNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as AssignVariableNodeData;
    
    // 构建赋值项列表显示
    const assignmentsHtml = nodeData.assignments
      .map((assignment: AssignmentItem, index: number) => {
        const variableName = assignment.targetVariableName || 'No variable';
        const expressionUnit = assignment.expressionUnit as any;
        const code = expressionUnit?.isFunctionMode 
          ? (expressionUnit?.functionCode || '') 
          : (expressionUnit?.expressionCode || '');
        const expression = code || 'Not set';
        const shortExpression = expression.length > 15 
          ? expression.substring(0, 15) + '...' 
          : expression;
          
        return `
          <div class="assignment-item">
            <div class="assignment-variable">${variableName}</div>
            <div class="assignment-operator">=</div>
            <div class="assignment-expression" title="${expression}">${shortExpression}</div>
          </div>
        `;
      })
      .join('');

    // 渲染节点HTML
    rootEl.innerHTML = `
      <style>
        .assign-variable-node .assignments-list {
          margin-top: 8px;
        }
        
        .assign-variable-node .assignment-item {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 6px 10px;
          margin-bottom: 6px;
          background: #f8fafc;
          border: 1px solid #e2e8f0;
          border-radius: 4px;
          font-size: 11px;
        }
        
        .assign-variable-node .assignment-item:last-child {
          margin-bottom: 0;
        }
        
        .assign-variable-node .assignment-variable {
          flex-shrink: 0;
          font-weight: 600;
          color: #059669;
          font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        }
        
        .assign-variable-node .assignment-operator {
          flex-shrink: 0;
          color: #6b7280;
          font-weight: bold;
        }
        
        .assign-variable-node .assignment-expression {
          flex: 1;
          color: #374151;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
          font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        }
      </style>
      <div class="dify-node assign-variable-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="m199.04 672.64 193.984 112 224-387.2-193.92-112-224 387.2zm-23.872 60.16 32.896 148.288 144.896-45.696L175.168 732.8zM455.04 229.248l193.92 112 56.704-98.112-193.984-112-56.64 98.112zM104.32 708.8l384-665.024 304.768 175.936L409.152 884.8h-.064l-248.448 78.336L104.32 708.8zm384 254.272v-64h448v64h-448z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'Assign'}</span>
        </div>
        <div class="node-info">
          <div class="assignments-list">
            ${assignmentsHtml}
          </div>
        </div>
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: AssignVariableNodeView,
  model: AssignVariableNodeModel,
};
