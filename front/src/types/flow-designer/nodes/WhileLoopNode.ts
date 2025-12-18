
import { h, ModelType } from '@logicflow/core';
import { GroupNode, GroupNodeModel } from '@logicflow/extension';
import type { NodeBase } from '../NodeBase';
import type { JSExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

const typeName = 'WhileLoopNode';

/**
 * While 循环节点数据结构
 */
export interface WhileLoopNodeData extends NodeBase {
  typeName: 'WhileLoopNode';
  /** 循环条件（JS 表达式，返回布尔值） */
  condition: JSExpressionUnit;
}

/**
 * While 循环节点模型
 * 继承自 GroupNodeModel，支持分组功能
 */
class WhileLoopNodeModel extends GroupNodeModel {
  constructor(data: any, graphModel: any) {
    super(data, graphModel);
    // 将模型类型标记为矩形，使 NodeResize 控件可用
    this.modelType = ModelType.RECT_NODE;
    // 设定缩放范围，避免被拖到过小或过大
    this.minWidth = 240;
    this.minHeight = 180;
    this.maxWidth = 1200;
    this.maxHeight = 900;
  }

  /**
   * 标记节点支持调整大小
   * 这是LogicFlow用于判断节点是否可以调整尺寸的方法
   */
  isResizable() {
    return true;
  }

  /**
   * 初始化节点数据
   */
  initNodeData(data: any) {
    super.initNodeData(data);
    
    // 设置分组属性
    this.isRestrict = true; // 禁止子节点拖出分组
    this.resizable = true; // 允许调整大小
    this.foldable = true; // 允许折叠
    this.width = 600;
    this.height = 400;
    
    // 设置节点默认数据
    const nodeData: Partial<WhileLoopNodeData> = {
      typeName,
      displayName: (this.properties.displayName as string | undefined) || 'While Loop',
      description: (this.properties.description as string | undefined) || 'Repeat child flow while condition is true',
      // 循环条件默认使用表达式 true
      condition: (this.properties.condition as JSExpressionUnit | undefined) || ExpressionUnitFactory.createJSExpression('', 'true'),
      // 容器节点默认没有输出
      hasOutput: false,
    };
    
    this.setNodeData(nodeData);
  }

  /**
   * 默认锚点：左侧输入、右侧输出（容器本身参与主流程）
   */
  getDefaultAnchor() {
    const { x, y, width, id } = this;
    return [
      {
        x: x - width / 2,
        y,
        id: `${id}_input`,
      },
      {
        x: x + width / 2,
        y,
        id: `${id}_output`,
      },
    ];
  }

  /**
   * 显示可见的锚点样式（便于连线）
   */
  getAnchorStyle(anchorInfo: any) {
    const style = super.getAnchorStyle(anchorInfo);
    style.r = 6;
    style.stroke = '#3b82f6';
    style.fill = '#ffffff';
    style.hover = {
      stroke: '#2563eb',
      fill: '#3b82f6',
    };
    return style;
  }

  /**
   * 设置节点样式
   */
  getNodeStyle() {
    const style = super.getNodeStyle();
    style.stroke = '#0ea5e9';
    style.strokeWidth = 2;
    style.fill = '#f0f9ff';
    style.fillOpacity = 0.5;
    
    if (this.isSelected) {
      style.stroke = '#0284c7';
      style.strokeWidth = 3;
    }
    
    if (this.isFolded) {
      style.fill = '#bae6fd';
    }
    
    return style;
  }

  /**
   * 折叠分组时的处理
   */
  foldGroup(folded: boolean) {
    super.foldGroup(folded);
    
    // 折叠时显示条件预览
    if (folded) {
      const nodeData = this.properties as WhileLoopNodeData;
      const condition = nodeData.condition;
      if (condition) {
        const expressionCode = condition.expressionCode || condition.functionCode || '';
        const preview = expressionCode.trim() || 'true';
        this.text = {
          ...this.text,
          value: `While: ${preview}`,
          x: this.x - this.width / 2 + 10,
          y: this.y,
        };
      }
    } else {
      // 展开时清除文本
      this.text = {
        ...this.text,
        value: '',
      };
    }
  }

  /**
   * 设置节点数据（重写以支持属性更新）
   */
  setNodeData(data: Partial<WhileLoopNodeData>) {
    this.properties = {
      ...this.properties,
      ...data,
    };
  }

  /**
   * 获取节点数据
   */
  getNodeData(): WhileLoopNodeData {
    return this.properties as WhileLoopNodeData;
  }
}

/**
 * While 循环节点视图
 * 继承自 GroupNode，使用默认的分组视图
 */
class WhileLoopNodeView extends GroupNode {
  getShape() {
    const { model } = this.props as any;
    const { x, y, width, height, properties } = model;
    const label = properties?.displayName || 'While Loop';
    // 标签放在节点上方，保持居中
    const labelY = y - height / 2 - 10;

    return h(
      'g',
      null,
      super.getShape(),
      h(
        'text',
        {
          x,
          y: labelY,
          'text-anchor': 'middle',
          'dominant-baseline': 'middle',
          fill: '#0ea5e9',
          'font-size': 14,
          'font-weight': '600',
        },
        label
      )
    );
  }
}

// LogicFlow 注册对象
export default {
  type: typeName,
  view: WhileLoopNodeView,
  model: WhileLoopNodeModel,
};

