
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase, NodeOutputItem } from '../NodeBase';
import { VariableItemType } from '../Parameters/Variable';
import type { AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

const typeName = 'JSCodeNode';

/**
 * JS代码节点数据接口
 */
export interface JSCodeNodeData extends NodeBase {
  /** 节点类型标识 */
  typeName: 'JSCodeNode';
  
  /** JS代码表达式单元 */
  codeUnit?: AnyExpressionUnit;
}

// JS代码节点模型
class JSCodeNodeModel extends NodeBaseModel {
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    
    // 默认JS代码模板
    const defaultCodeTemplate = `function main() {
  // use variables, for example: {{age}} 
  return {
    x: 1,
    y: "ok"
  };
}`;
    
    // 设置节点默认数据（只在属性不存在时设置默认值）
    const nodeData: Partial<JSCodeNodeData> = {
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "JS Code",
      description: (this.properties.description as string | undefined) || "Execute JavaScript code",
      hasOutput: true, // JS代码节点有输出
      // JS代码节点的输出结构（用户自定义，默认两个字段）
      outputs: (this.properties.outputs as NodeOutputItem[] | undefined) || [
        {
          name: 'x',
          variableType: VariableItemType.LongVariable
        },
        {
          name: 'y',
          variableType: VariableItemType.StringVariable
        }
      ],
      // 初始化JS代码单元（函数模式）
      codeUnit: (this.properties.codeUnit as AnyExpressionUnit | undefined) || (() => {
        const unit = ExpressionUnitFactory.createJSExpression(defaultCodeTemplate, '');
        unit.isFunctionMode = true; // 设置为函数模式
        return unit;
      })()
    };
    this.setNodeData(nodeData);
  }
}

// JS代码节点视图
class JSCodeNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as JSCodeNodeData;
    
    // 获取代码预览（取前30个字符）
    const codeUnit = nodeData.codeUnit as any;
    const code = codeUnit?.isFunctionMode 
      ? (codeUnit?.functionCode || '') 
      : (codeUnit?.expressionCode || '');
    const codePreview = code ? code.trim().substring(0, 30) + '...' : 'No code set';
    
    // 渲染节点HTML
    rootEl.innerHTML = `
      <div class="dify-node jscode-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="M832 384H576V128H192v768h640V384zm-26.496-64L640 154.496V320h165.504zM160 64h480l256 256v608a32 32 0 0 1-32 32H160a32 32 0 0 1-32-32V96a32 32 0 0 1 32-32zm160 448h384v64H320v-64zm0-192h160v64H320v-64zm0 384h384v64H320v-64z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'JS Code'}</span>
        </div>
        <div class="node-info">
          <div class="info-text" title="${code}">${codePreview}</div>
        </div>
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: JSCodeNodeView,
  model: JSCodeNodeModel,
};
