
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';
import { VariableItemType } from '../Parameters/Variable';
import type { AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

const typeName = 'LLMNode'

/**
 * LLM节点数据接口
 * 扩展基础节点数据结构，添加LLM节点特有的属性
 */
export interface LLMNodeData extends NodeBase {
  /** 模型选择 - 格式：平台名称|模型名称，例如："OpenAI|gpt-4" */
  modelSelection?: string;
  
  /** 温度参数，控制生成的随机性，0-1之间，默认0.7 */
  temperature?: number;
  
  /** System角色提示词 */
  systemPrompt?: AnyExpressionUnit;
  
  /** User角色提示词 */
  userPrompt?: AnyExpressionUnit;
  
  /** 是否启用记忆功能，默认false */
  memoryEnabled?: boolean;
  
  /** 记忆轮数，当memoryEnabled为true时有效，默认5 */
  memoryRounds?: number;
  
  /** 是否启用深度思考模式，默认false */
  enableThinking?: boolean;

  /** 
   * 要求所选择的大模型必须支持图片解析; 图片必须是公网图片地址;
   * 图片, 支持 [ {url: "公网图片地址"}, {url: "公网图片地址"} ] 格式; 
   * 系统变量sys.Files的结构是  [{url: string; name?: string; size?: number; mimeType?: string;}, ...],可以直接使用, 后台会忽略其他的属性
   * */
  pictures?: AnyExpressionUnit;
}

// LLM节点模型
class LLMNodeModel extends NodeBaseModel {
  // 计算节点高度的辅助方法
  private calculateHeight(): number {
    const baseHeight = 90;
    const hasSystemPrompt = this.properties.systemPrompt ? 20 : 0;
    const hasUserPrompt = this.properties.userPrompt ? 20 : 0;
    return baseHeight + hasSystemPrompt + hasUserPrompt;
  }
  
  // 设置节点样式和属性
  setAttributes() {
    super.setAttributes(); // 调用父类方法设置默认属性
    
    // 设置节点高度
    this.height = this.calculateHeight();
    
    // 设置节点默认数据（只在属性不存在时设置默认值）
    const nodeData: Partial<LLMNodeData> = {
      typeName: typeName,
      displayName: (this.properties.displayName as string | undefined) || "LLM",
      description: (this.properties.description as string | undefined) || "Call large language model",
      hasOutput: true, // LLM节点有输出
      // LLM节点的输出结构（固定）
      outputs: [
        {
          name: 'text',
          variableType: VariableItemType.StringVariable
        }
      ],
      modelSelection: (this.properties.modelSelection as string | undefined) || '',
      temperature: (this.properties.temperature as number | undefined) !== undefined ? (this.properties.temperature as number) : 0.7,
      systemPrompt: (this.properties.systemPrompt as AnyExpressionUnit | undefined) || ExpressionUnitFactory.createFullTextExpression(''),
      userPrompt: (this.properties.userPrompt as AnyExpressionUnit | undefined) || ExpressionUnitFactory.createFullTextExpression(''),
      memoryEnabled: (this.properties.memoryEnabled as boolean | undefined) !== undefined ? (this.properties.memoryEnabled as boolean) : false,
      memoryRounds: (this.properties.memoryRounds as number | undefined) !== undefined ? (this.properties.memoryRounds as number) : 5,
      enableThinking: (this.properties.enableThinking as boolean | undefined) !== undefined ? (this.properties.enableThinking as boolean) : false,
      pictures: (this.properties.pictures as AnyExpressionUnit | undefined) || ExpressionUnitFactory.createJSExpression('')
    };
    this.setNodeData(nodeData);
  }
  
  // 重写setProperties方法，当属性变化时重新计算高度
  setProperties(properties: Record<string, any>) {
    super.setProperties(properties);
    // 当提示词属性变化时，重新计算高度
    this.height = this.calculateHeight();
  }
}

// LLM节点视图
class LLMNodeView extends HtmlNode {
  // 渲染节点的HTML
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as LLMNodeData;
    
    // 提取模型名称显示（如果有选择的话）
    let modelDisplayName = 'No model';
    if (nodeData.modelSelection) {
      const parts = nodeData.modelSelection.split('|');
      if (parts.length === 2) {
        modelDisplayName = parts[1]; // 只显示模型名称部分
      }
    }
    
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
    
    // 处理提示词预览（移除{{}}变量标记，只显示纯文本，截断过长文本）
    const formatPromptPreview = (prompt: string, maxLength: number = 30): string => {
      if (!prompt) return '';
      // 移除变量标记 {{variableName}}
      const cleanText = prompt.replace(/\{\{[^}]+\}\}/g, '{var}');
      // 截断并添加省略号
      return cleanText.length > maxLength 
        ? cleanText.substring(0, maxLength) + '...' 
        : cleanText;
    };
    
    const systemPreview = formatPromptPreview(extractTextFromExpressionUnit(nodeData.systemPrompt));
    const userPreview = formatPromptPreview(extractTextFromExpressionUnit(nodeData.userPrompt));
    
    // 新样式：上方图标+名称，下方显示信息
    rootEl.innerHTML = `
      <div class="dify-node llm-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="M512 64a448 448 0 1 1 0 896 448 448 0 0 1 0-896zm0 832a384 384 0 0 0 0-768 384 384 0 0 0 0 768zm48-176a48 48 0 1 1-96 0 48 48 0 0 1 96 0zm-48-464c-96.128 0-160 64-160 160 0 17.664 14.336 32 32 32s32-14.336 32-32c0-59.136 35.2-96 96-96s96 36.864 96 96-35.2 96-96 96a32 32 0 0 0-32 32v96a32 32 0 0 0 64 0v-67.2c73.472-9.728 128-70.144 128-156.8 0-96-63.872-160-160-160z"/>
            </svg>
          </div>
          <span class="node-name">${nodeData.displayName || 'LLM'}</span>
        </div>
        <div class="node-info">
          <span class="info-text">${modelDisplayName} | Temp: ${nodeData.temperature}</span>
        </div>
        ${systemPreview ? `<div class="node-info"><span class="info-text">System: ${systemPreview}</span></div>` : ''}
        ${userPreview ? `<div class="node-info"><span class="info-text">User: ${userPreview}</span></div>` : ''}
      </div>
    `;
  }
}

// 下面这种导出格式是logicflow要求的, 用来注册到画布中
export default {
  type: typeName, // 与typeName保持一致，用于LogicFlow注册
  view: LLMNodeView,
  model: LLMNodeModel,
};
