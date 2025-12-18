/**
 * 开始节点
 * 流程的起点节点，参考Dify风格设计
 */
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';

const typeName = 'StartNode'

/**
 * 开始节点数据接口
 * 扩展基础节点数据结构，添加开始节点特有的属性
 */
export interface StartNodeData extends NodeBase {
  // 暂无特有属性

}

// 开始节点模型
class StartNodeModel extends NodeBaseModel {
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    // 设置节点默认数据
    const nodeData: Partial<StartNodeData> = {
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "Start"
    };
    this.setNodeData(nodeData);
  }

  // 定义节点的连接规则
  // 开始节点只能作为源节点（输出），不能作为目标节点（输入）
  getConnectedTargetRules() {
    const rules = super.getConnectedTargetRules();
    const notAsTarget = {
      message: 'Start node cannot be a connection target',
      validate: () => false,
    };
    rules.push(notAsTarget);
    return rules;
  }
}

// 开始节点视图
class StartNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as StartNodeData;
    
    // 新样式：上方图标+名称+标签，下方显示信息
    rootEl.innerHTML = `
      <div class="dify-node start-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="M512 64a448 448 0 1 1 0 896 448 448 0 0 1 0-896zm0 832a384 384 0 0 0 0-768 384 384 0 0 0 0 768zm-48-247.616L668.608 512 464 375.616v272.768zm10.624-342.656 249.472 166.336a48 48 0 0 1 0 79.872L474.624 718.272A48 48 0 0 1 400 678.336V345.6a48 48 0 0 1 74.624-39.936z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'Start'}</span>
        </div>
        <div class="node-info">
          <span class="info-text">${nodeData.description || 'Flow entry point'}</span>
        </div>
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: StartNodeView,
  model: StartNodeModel,
};
