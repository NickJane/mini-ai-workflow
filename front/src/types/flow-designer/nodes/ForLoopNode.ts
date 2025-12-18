
import LogicFlow from '@logicflow/core';
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase } from '../NodeBase';
import type { JSExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';
import type { NodeLine } from '../NodeLine';
import StartNode from './StartNode';

const typeName = 'ForLoopNode';

/**
 * For 循环内部子节点结构
 * 仅用于保存子流程信息，后续可在属性/子画布中继续扩展
 */
export interface ForLoopInnerNode extends NodeBase {
  /** 便于可视化定位的坐标（预留） */
  x?: number;
  y?: number;
  /** 兼容 LogicFlow 的类型字段（预留） */
  type?: string;
}

/**
 * For 循环节点数据结构
 */
export interface ForLoopNodeData extends NodeBase {
  typeName: 'ForLoopNode';
  /** 循环次数（JS 表达式） */
  loopCount: JSExpressionUnit;
  /** 子流程节点列表（默认包含一个开始节点） */
  innerNodes: ForLoopInnerNode[];
  /** 子流程连线列表（预留，默认空） */
  innerLines: NodeLine[];
}

// 默认内部开始节点生成器
const createDefaultInnerStart = (parentId: string): ForLoopInnerNode => ({
  id: `${parentId}_inner_start`,
  typeName: 'StartNode',
  displayName: 'Start',
  description: 'Loop entry',
  x: 120,
  y: 80
});

// For 循环节点模型
class ForLoopNodeModel extends NodeBaseModel {
  /**
   * 标记节点支持调整大小
   * 这是LogicFlow用于判断节点是否可以调整尺寸的方法
   */
  isResizable() {
    return true;
  }

  // 设置节点默认属性（支持外部传入宽高，用于拖拽缩放）
  setAttributes(attributes?: { width?: number; height?: number }) {
    // 先调用父类设置基础属性
    super.setAttributes(attributes);

    // 容器节点默认 600x400，允许外部传入覆盖
    this.width = attributes?.width ?? 600;
    this.height = attributes?.height ?? 400;

    // 缺省时自动补齐内部开始节点
    const existingInnerNodes = this.properties.innerNodes as ForLoopInnerNode[] | undefined;
    const innerNodes =
      (existingInnerNodes && existingInnerNodes.length > 0)
        ? existingInnerNodes
        : [createDefaultInnerStart(this.id)];

    const nodeData: Partial<ForLoopNodeData> = {
      typeName,
      displayName: (this.properties.displayName as string | undefined) || 'For Loop',
      description: (this.properties.description as string | undefined) || 'Repeat child flow by times',
      // 循环次数默认使用表达式 10
      loopCount: (this.properties.loopCount as JSExpressionUnit | undefined) || ExpressionUnitFactory.createJSExpression('', '10'),
      // 容器节点默认没有输出
      hasOutput: false,
      // 默认携带一个内部开始节点，后续可扩展
      innerNodes,
      innerLines: (this.properties.innerLines as NodeLine[] | undefined) || []
    };

    this.setNodeData(nodeData);
  }

  /**
   * 左右各一个锚点：左进右出
   */
  getDefaultAnchor() {
    const { width, height, x, y, id } = this;
    return [
      {
        x: x - width / 2,
        y,
        id: `${id}_input`
      },
      {
        x: x + width / 2,
        y,
        id: `${id}_output`
      }
    ];
  }
}

// For 循环节点视图
class ForLoopNodeView extends HtmlNode {
  private innerLf: LogicFlow | null = null;

  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const nodeData = properties as ForLoopNodeData;

    // 取循环次数预览文本（表达式模式）
    const loopExpression = (nodeData.loopCount?.expressionCode || '').trim() || '10';

    rootEl.innerHTML = `
      <style>
        .forloop-node {
          border: 1px solid #e5e7eb;
          border-radius: 10px;
          background: #ffffff;
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
          padding: 12px;
          box-sizing: border-box;
          height: 100%;
          display: flex;
          flex-direction: column;
          position: relative;
        }
        .forloop-node .node-header {
          display: flex;
          align-items: center;
          gap: 10px;
          margin-bottom: 8px;
        }
        .forloop-node .node-icon {
          width: 28px;
          height: 28px;
          border-radius: 6px;
          display: flex;
          align-items: center;
          justify-content: center;
          color: #ffffff;
          background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%);
        }
        .forloop-node .node-name {
          font-size: 14px;
          font-weight: 600;
          color: #111827;
        }
        .forloop-node .node-desc {
          font-size: 12px;
          color: #6b7280;
        }
        .forloop-node .loop-meta {
          display: flex;
          align-items: center;
          gap: 6px;
          margin: 10px 0;
          font-size: 12px;
          color: #374151;
        }
        .forloop-node .loop-meta .label {
          font-weight: 600;
          color: #2563eb;
        }
        .forloop-node .loop-body {
          flex: 1;
          min-height: 200px;
          border: 1px dashed #cbd5e1;
          border-radius: 8px;
          background: linear-gradient(180deg, #f8fafc 0%, #ffffff 100%);
          display: flex;
          flex-direction: column;
          padding: 8px;
          box-sizing: border-box;
          gap: 6px;
        }
        .forloop-node .inner-start {
          display: inline-flex;
          align-items: center;
          gap: 6px;
          padding: 6px 10px;
          border-radius: 999px;
          background: #e0f2fe;
          color: #0369a1;
          font-weight: 600;
          box-shadow: inset 0 0 0 1px #bae6fd;
        }
        .forloop-node .inner-start svg {
          color: #0284c7;
        }
        .forloop-node .inner-canvas {
          flex: 1;
          width: 100%;
          border: 1px solid #e5e7eb;
          border-radius: 6px;
          background: #ffffff;
          overflow: hidden;
        }
        .forloop-node .resize-handle {
          position: absolute;
          width: 14px;
          height: 14px;
          right: 4px;
          bottom: 4px;
          border-radius: 3px;
          background: #3b82f6;
          cursor: se-resize;
          box-shadow: 0 1px 3px rgba(0,0,0,0.25);
        }
      </style>
      <div class="dify-node forloop-node">
        <div class="node-header">
          <div class="node-icon">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="16" height="16">
              <path fill="currentColor" d="M512 64c247.36 0 448 200.64 448 448s-200.64 448-448 448S64 759.36 64 512c0-53.12 43.52-96 96-96s96 42.88 96 96c0 106.08 85.92 192 192 192s192-85.92 192-192-85.92-192-192-192c-35.52 0-64-28.64-64-64s28.48-64 64-64c176.64 0 320 143.36 320 320s-143.36 320-320 320S192 688.64 192 512c0-176.48 143.52-320 320-320z"/>
            </svg>
          </div>
          <div>
            <div class="node-name">${nodeData.displayName || 'For Loop'}</div>
            <div class="node-desc">${nodeData.description || 'Loop container, drop nodes inside'}</div>
          </div>
        </div>
        <div class="loop-meta">
          <span class="label">Times</span>
          <span>${loopExpression}</span>
        </div>
        <div class="loop-body">
          <div class="inner-canvas" id="inner-canvas-${this.props.model.id}"></div>
        </div>
        <div class="resize-handle" data-role="resize"></div>
      </div>
    `;

    // 初始化内部 LogicFlow 画布
    const canvasId = `inner-canvas-${this.props.model.id}`;
    this.initInnerCanvas(canvasId, nodeData);
    this.bindInnerCanvasEvents(canvasId);
    this.bindResizeHandle(rootEl);
  }

  /**
   * 初始化内部子画布，支持选中/拖拽，数据同步到节点属性
   */
  private initInnerCanvas(containerId: string, nodeData: ForLoopNodeData) {
    const container = document.getElementById(containerId);
    if (!container) return;

    // 复用或重建实例（兼容场景：render 触发多次 setHtml）
    if (this.innerLf && typeof (this.innerLf as any).destroy === 'function') {
      (this.innerLf as any).destroy();
    }
    this.innerLf = null;

    const lf = new LogicFlow({
      container,
      grid: {
        type: 'mesh',
        size: 12,
        visible: true,
      },
      background: {
        color: '#f8fafc',
      },
      stopScrollGraph: true,
      stopZoomGraph: true,
    });

    // 注册可用节点（当前仅 Start，后续可扩展）
    lf.register(StartNode as any);

    // 准备图数据
    const nodes = (nodeData.innerNodes || []).map(n => ({
      id: n.id,
      type: n.typeName,
      x: n.x ?? 120,
      y: n.y ?? 80,
      properties: { ...n, typeName: n.typeName, displayName: n.displayName || 'Start' },
    }));

    const edges = (nodeData.innerLines || []).map(line => ({
      id: line.id,
      type: 'polyline',
      sourceNodeId: line.fromNodeId,
      targetNodeId: line.toNodeId,
      sourceAnchorId: line.sourceAnchorId,
      targetAnchorId: line.targetAnchorId,
    }));

    lf.render({ nodes, edges });

    const syncToNodeProperties = () => {
      const data = lf.getGraphData() as { nodes: any[]; edges: any[] };
      const innerNodes: ForLoopInnerNode[] = data.nodes.map((n: any) => ({
        id: n.id,
        typeName: n.type || n.properties?.typeName || 'StartNode',
        displayName: n.text?.value || n.properties?.displayName || 'Start',
        description: n.properties?.description,
        x: n.x,
        y: n.y,
      }));
      const innerLines: NodeLine[] = (data.edges || []).map((e: any) => ({
        id: e.id,
        fromNodeId: e.sourceNodeId,
        toNodeId: e.targetNodeId,
        sourceAnchorId: e.sourceAnchorId,
        targetAnchorId: e.targetAnchorId,
      }));
      const model = this.props.model as ForLoopNodeModel;
      model.setProperties({
        innerNodes,
        innerLines,
      });
    };

    // 拖拽结束时限制在子画布范围内，避免拖出后持续跟随
    const clampOnDragEnd = ({ data }: any) => {
      if (!data) return;
      const rect = container.getBoundingClientRect();
      const halfW = rect.width / 2;
      const halfH = rect.height / 2;
      const model = lf.getNodeModelById(data.id);
      const nodeW = (model as any)?.width ?? (data?.width ?? 240);
      const nodeH = (model as any)?.height ?? (data?.height ?? 90);
      const halfNodeW = nodeW / 2;
      const halfNodeH = nodeH / 2;
      const minX = -halfW + halfNodeW + 4;
      const maxX = halfW - halfNodeW - 4;
      const minY = -halfH + halfNodeH + 4;
      const maxY = halfH - halfNodeH - 4;
      const clampedX = Math.max(minX, Math.min(maxX, data.x));
      const clampedY = Math.max(minY, Math.min(maxY, data.y));
      if (model && (clampedX !== model.x || clampedY !== model.y)) {
        if (typeof (model as any).moveTo === 'function') {
          (model as any).moveTo(clampedX, clampedY);
        } else {
          model.x = clampedX;
          model.y = clampedY;
        }
      }
      syncToNodeProperties();
    };

    lf.on('node:dragend', clampOnDragEnd);
    lf.on('edge:add', syncToNodeProperties);
    lf.on('edge:delete', syncToNodeProperties);

    this.innerLf = lf;
  }

  /**
   * 阻止内部画布事件冒泡到外层，避免拖拽时外层容器跟随
   * 不调用 preventDefault，保留内部逻辑流的正常拖拽
   */
  private bindInnerCanvasEvents(containerId: string) {
    const container = document.getElementById(containerId);
    if (!container) return;
    const stop = (e: Event) => e.stopPropagation();
    // 阻止按下等事件冒泡，释放（mouseup/touchend）不拦截，确保LogicFlow能正确结束拖拽
    ['mousedown', 'click', 'dblclick', 'contextmenu', 'wheel', 'touchstart'].forEach(evt =>
      container.addEventListener(evt, stop, false)
    );
  }

  /**
   * 绑定右下角拖拽缩放
   */
  private bindResizeHandle(rootEl: SVGForeignObjectElement) {
    const handle = rootEl.querySelector('.resize-handle') as HTMLElement | null;
    if (!handle) return;
    const model = this.props.model as ForLoopNodeModel;
    const canvasEl = rootEl.querySelector('.inner-canvas') as HTMLElement | null;

    const minW = 360;
    const minH = 260;
    const maxW = 1000;
    const maxH = 800;

    const onMouseDown = (e: MouseEvent) => {
      e.stopPropagation();
      const startX = e.clientX;
      const startY = e.clientY;
      const startW = model.width;
      const startH = model.height;

      const onMouseMove = (ev: MouseEvent) => {
        ev.stopPropagation();
        const dw = ev.clientX - startX;
        const dh = ev.clientY - startY;
        const newW = Math.max(minW, Math.min(maxW, startW + dw));
        const newH = Math.max(minH, Math.min(maxH, startH + dh));
        if (newW !== model.width || newH !== model.height) {
          // 直接更新模型的宽高
          model.width = newW;
          model.height = newH;
          
          // 手动更新DOM元素尺寸（HTML节点需要手动同步DOM）
          if (rootEl) {
            rootEl.style.width = `${newW}px`;
            rootEl.style.height = `${newH}px`;
            
            // 找到foreignObject父元素并更新其尺寸属性
            const foreignObject = rootEl.parentElement;
            if (foreignObject && foreignObject.tagName === 'foreignObject') {
              foreignObject.setAttribute('width', String(newW));
              foreignObject.setAttribute('height', String(newH));
            }
          }
          
          // 触发视图刷新和锚点重算
          model.setProperties({
            ...model.properties,
            __forLoopResizeVersion: Date.now(),
          });
          
          // 同步子画布尺寸，避免白板
          if (canvasEl && this.innerLf) {
            requestAnimationFrame(() => {
              const w = canvasEl.clientWidth;
              const h = canvasEl.clientHeight;
              if (w > 0 && h > 0) {
                this.innerLf?.resize(w, h);
              }
            });
          }
        }
      };

      const onMouseUp = (ev: MouseEvent) => {
        ev.stopPropagation();
        window.removeEventListener('mousemove', onMouseMove, true);
        window.removeEventListener('mouseup', onMouseUp, true);
      };

      window.addEventListener('mousemove', onMouseMove, true);
      window.addEventListener('mouseup', onMouseUp, true);
    };

    handle.addEventListener('mousedown', onMouseDown, true);
  }
}

// LogicFlow 注册对象
export default {
  type: typeName,
  view: ForLoopNodeView,
  model: ForLoopNodeModel,
};