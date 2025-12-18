/**
 * 生成UUID工具函数
 */

/**
 * 生成UUID v4
 * @returns 返回格式化的UUID字符串
 */
export function generateUUID(): string {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

/**
 * 生成基于时间戳的唯一ID
 * @param prefix 可选的前缀
 * @returns 返回唯一ID字符串
 */
export function generateTimestampId(prefix?: string): string {
  const timestamp = Date.now();
  const random = Math.random().toString(36).substring(2, 8);
  return prefix ? `${prefix}_${timestamp}_${random}` : `${timestamp}_${random}`;
}

/**
 * 生成节点专用的唯一ID
 * @param nodeType 节点类型
 * @returns 返回节点ID
 */
export function generateNodeId(nodeType: string): string {
  return generateTimestampId(nodeType.toLowerCase());
}
