/**
 * 回复节点
 * 用于向用户发送回复消息，参考Dify风格设计
 */
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';
import type { AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

const typeName = 'ReplyNode'
/**
 * 回复节点数据接口
 * 扩展基础节点数据结构，添加回复节点特有的属性
 */
export interface ReplyNodeData extends NodeBase {
  /** 回复消息内容 */
  message: AnyExpressionUnit;
}

// 回复节点模型
class ReplyNodeModel extends NodeBaseModel {
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    // 回复节点不需要缩放
    this.resizable = false;
    // 设置节点默认数据（只在属性不存在时设置默认值）
    const nodeData: Partial<ReplyNodeData> = {
      message: (this.properties.message as AnyExpressionUnit | undefined) || ExpressionUnitFactory.createFullTextExpression(''),
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "Reply"
    };
    this.setNodeData(nodeData);
  }
}

// 回复节点视图
class ReplyNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as ReplyNodeData;
    
    // 从表达式单元中提取文本内容
    const extractTextFromExpressionUnit = (unit: AnyExpressionUnit | string | undefined): string => {
      if (!unit) return '';
      if (typeof unit === 'string') return unit; // 兼容旧数据
      
      // 根据表达式单元类型提取文本
      if (unit.typeName === 'FullTextExpressionUnit') {
        return (unit as any).Text || '';
      } else if (unit.typeName === 'JSExpressionUnit') {
        const jsUnit = unit as any;
        return jsUnit.isFunctionMode ? jsUnit.functionCode : jsUnit.expressionCode;
      }
      return '';
    };
    
    const messageText = extractTextFromExpressionUnit(nodeData.message);
    const messagePreview = messageText ? `: ${messageText.substring(0, 20)}${messageText.length > 20 ? '...' : ''}` : '';
    
    // 新样式：上方图标+名称+标签，下方显示信息
    rootEl.innerHTML = `
      <div class="dify-node reply-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="M273.536 736H800a64 64 0 0 0 64-64V256a64 64 0 0 0-64-64H224a64 64 0 0 0-64 64v570.88L273.536 736zM296 800 147.968 918.4A32 32 0 0 1 96 893.44V256a128 128 0 0 1 128-128h576a128 128 0 0 1 128 128v416a128 128 0 0 1-128 128H296z"/>
              <path fill="currentColor" d="M512 499.2a51.2 51.2 0 1 1 0-102.4 51.2 51.2 0 0 1 0 102.4zm192 0a51.2 51.2 0 1 1 0-102.4 51.2 51.2 0 0 1 0 102.4zm-384 0a51.2 51.2 0 1 1 0-102.4 51.2 51.2 0 0 1 0 102.4z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'Reply'}</span>
        </div>
        <div class="node-info">
          <span class="info-text">${nodeData.description || `Send reply${messagePreview}`}</span>
        </div>
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: ReplyNodeView,
  model: ReplyNodeModel,
};
