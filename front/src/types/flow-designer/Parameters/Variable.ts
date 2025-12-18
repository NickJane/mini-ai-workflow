/**
 * 流程变量系统类型定义
 * 参照后端C#设计，提供前端基础结构
 */

/**
 * 变量类型枚举
 */
export enum VariableItemType {
  LongVariable = 'LongVariable',
  DecimalVariable = 'DecimalVariable',
  StringVariable = 'StringVariable',
  ObjectVariable = 'ObjectVariable',
  DateTimeVariable = 'DateTimeVariable',
  BooleanVariable = 'BooleanVariable',
  ArrayVariable = 'ArrayVariable'
}

/**
 * 变量基类接口
 * 定义所有变量的共同属性和结构
 */
export interface Variable {
  /** 变量唯一标识 */
  id?: string;
  
  /** 变量名称 */
  name: string;
  
  /** 是否必填 */
  required?: boolean;
  
  /** 默认值 */
  defaultValue?: any;
  
  /** 是否已设置值（前端暂不使用，保持与后端结构一致） */
  hasValue?: boolean;
  
  /** 变量类型标识，用于序列化 */
  typeName: VariableItemType;
}

/**
 * 字符串变量
 */
export interface StringVariable extends Variable {
  typeName: VariableItemType.StringVariable;
  defaultValue?: string;
}

/**
 * 数字变量（长整型）
 */
export interface LongVariable extends Variable {
  typeName: VariableItemType.LongVariable;
  defaultValue?: number;
}

/**
 * 浮点数变量
 */
export interface DecimalVariable extends Variable {
  typeName: VariableItemType.DecimalVariable;
  defaultValue?: number;
}

/**
 * 布尔变量
 */
export interface BooleanVariable extends Variable {
  typeName: VariableItemType.BooleanVariable;
  defaultValue?: boolean;
}

/**
 * 日期时间变量
 */
export interface DateTimeVariable extends Variable {
  typeName: VariableItemType.DateTimeVariable;
  defaultValue?: string; // ISO日期字符串格式
}

/**
 * 对象变量
 */
export interface ObjectVariable extends Variable {
  typeName: VariableItemType.ObjectVariable;
  /** 对象的子属性列表 */
  children?: Variable[];
  defaultValue?: Record<string, any>;
}

/**
 * 数组变量
 */
export interface ArrayVariable extends Variable {
  typeName: VariableItemType.ArrayVariable;
  /** 数组元素的类型 */
  itemType?: VariableItemType;
  /** 数组元素模板（当itemType为ObjectVariable时使用） */
  children?: Variable[];
  defaultValue?: any[];
}

/**
 * 变量联合类型
 */
export type AnyVariable = 
  | StringVariable 
  | LongVariable 
  | DecimalVariable 
  | BooleanVariable 
  | DateTimeVariable 
  | ObjectVariable 
  | ArrayVariable;

/**
 * 创建变量的工厂函数
 */
export class VariableFactory {
  /**
   * 创建字符串变量
   */
  static createStringVariable(name: string, required = false, defaultValue?: string): StringVariable {
    return {
      name,
      required,
      defaultValue,
      typeName: VariableItemType.StringVariable,
      hasValue: false
    };
  }

  /**
   * 创建数字变量
   */
  static createLongVariable(name: string, required = false, defaultValue?: number): LongVariable {
    return {
      name,
      required,
      defaultValue,
      typeName: VariableItemType.LongVariable,
      hasValue: false
    };
  }

  /**
   * 创建浮点数变量
   */
  static createDecimalVariable(name: string, required = false, defaultValue?: number): DecimalVariable {
    return {
      name,
      required,
      defaultValue,
      typeName: VariableItemType.DecimalVariable,
      hasValue: false
    };
  }

  /**
   * 创建布尔变量
   */
  static createBooleanVariable(name: string, required = false, defaultValue?: boolean): BooleanVariable {
    return {
      name,
      required,
      defaultValue,
      typeName: VariableItemType.BooleanVariable,
      hasValue: false
    };
  }

  /**
   * 创建日期时间变量
   */
  static createDateTimeVariable(name: string, required = false, defaultValue?: string): DateTimeVariable {
    return {
      name,
      required,
      defaultValue,
      typeName: VariableItemType.DateTimeVariable,
      hasValue: false
    };
  }

  /**
   * 创建对象变量
   */
  static createObjectVariable(name: string, children: Variable[] = [], required = false, defaultValue?: Record<string, any>): ObjectVariable {
    return {
      name,
      required,
      defaultValue,
      children,
      typeName: VariableItemType.ObjectVariable,
      hasValue: false
    };
  }

  /**
   * 创建数组变量
   */
  static createArrayVariable(name: string, itemType: VariableItemType, required = false, children: Variable[] = [], defaultValue?: any[]): ArrayVariable {
    return {
      name,
      required,
      defaultValue,
      itemType,
      children,
      typeName: VariableItemType.ArrayVariable,
      hasValue: false
    };
  }
}
