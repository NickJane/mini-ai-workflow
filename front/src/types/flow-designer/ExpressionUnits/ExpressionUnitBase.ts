/**
 * 表达式单元基类
 * 对应 C# 中的 ExpressionUnitBase
 */
export interface ExpressionUnitBase {
  /** 单元唯一标识 */
  id: string;
  
  /** 类型名称，用于多态序列化（对应C#的JsonConverter） */
  typeName: string;
  
  /** 子单元列表（支持嵌套） */
  children?: ExpressionUnitBase[];
}

/**
 * 所有表达式单元类型的联合类型
 * 方便后续扩展其他类型的表达式单元
 */
export type AnyExpressionUnit = JSExpressionUnit | FullTextMiniExpressionUnit | FullTextExpressionUnit;

/**
 * 表达式单元工厂
 * 用于创建各种类型的表达式单元
 */
export class ExpressionUnitFactory {
  /**
   * 创建 JS 表达式单元
   */
  static createJSExpression(functionCode: string = '', expressionCode: string = ''): JSExpressionUnit {
    return {
      id: `jsexpr_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      typeName: 'JSExpressionUnit',
      functionCode: functionCode,
      expressionCode: expressionCode,
      isFunctionMode: false,
      children: []
    };
  }

  /**
   * 创建迷你富文本表达式单元（单行/输入框展示）
   */
  static createFullTextMiniExpression(text: string = ''): FullTextMiniExpressionUnit {
    return {
      id: `ftmini_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      typeName: 'FullTextMiniExpressionUnit',
      Text: text,
      children: []
    };
  }

  /**
   * 创建富文本表达式单元
   */
  static createFullTextExpression(text: string = ''): FullTextExpressionUnit {
    return {
      id: `fulltext_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      typeName: 'FullTextExpressionUnit',
      Text: text,
      children: []
    };
  }
}


/**
 * JS表达式单元
 * 对应 C# 中的 JSExpressionUnit
 * 支持两种模式:
 * 1, js函数模式, 必须有main方法, 样例 function main() {
 * // 返回 true 或 false
  // 可以使用变量，例如：{{age}} > 18
  return {{age}} > 100;
  2. 表达式模式, 样例 {{age}} > 18
 */
export interface JSExpressionUnit extends ExpressionUnitBase {
  /** 类型标识 */
  typeName: 'JSExpressionUnit';
  
  /** JS函数字符串 */
  functionCode: string;
  /** 表达式代码字符串 */
  expressionCode: string;

  /** 表达式模式 */
  isFunctionMode: boolean;
}

/**
 * 迷你富文本表达式单元（输入框展示）
 */
export interface FullTextMiniExpressionUnit extends ExpressionUnitBase {
  /** 类型标识 */
  typeName: 'FullTextMiniExpressionUnit';
  
  /** 富文本内容 */
  Text: string;
}

/**
 * 前端富文本表达式单元
    富文本内目前含有下面三种占位符
    {{变量名}}
    {{sys.xxx}}系统变量;
    {{节点id.属性}} 获取节点输出结果.
 
    我们需要把{{}}中的内容解析出来，然后替换为对应的值, 返回出去
 */
export interface FullTextExpressionUnit extends ExpressionUnitBase {
  /** 类型标识 */
  typeName: 'FullTextExpressionUnit';
  
  /** 富文本内容 */
  Text: string;
}