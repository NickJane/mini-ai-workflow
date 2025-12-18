
import { HtmlNode } from '@logicflow/core';
import { NodeBaseModel } from '../NodeBase';
import type { NodeBase, NodeOutputItem } from '../NodeBase';
import { VariableItemType } from '../Parameters/Variable';
import type { FullTextMiniExpressionUnit, FullTextExpressionUnit, AnyExpressionUnit } from '../ExpressionUnits/ExpressionUnitBase';
import { ExpressionUnitFactory } from '../ExpressionUnits/ExpressionUnitBase';

const typeName = 'HttpNode';

// 仅支持 GET / POST
export type HttpMethod = 'GET' | 'POST';

export interface HttpNodeData extends NodeBase {
  typeName: 'HttpNode';
  method?: HttpMethod;
  url?: FullTextMiniExpressionUnit;
  headers?: FullTextExpressionUnit;
  query?: FullTextExpressionUnit;
  body?: AnyExpressionUnit;
  timeoutSeconds?: number;
}

class HttpNodeModel extends NodeBaseModel {
  private calcHeight(): number {
    // 基础高度 + 有 body 时适当增高
    const base = 110;
    const bodyExtra = this.properties.body ? 20 : 0;
    return base + bodyExtra;
  }

  setAttributes() {
    super.setAttributes();

    const defaultUrl = ExpressionUnitFactory.createFullTextMiniExpression('https://api.example.com');
    const defaultHeaders = ExpressionUnitFactory.createFullTextExpression('Content-Type: application/json');
    const defaultQuery = ExpressionUnitFactory.createFullTextExpression('?name=abc');
    const defaultBody = ExpressionUnitFactory.createFullTextExpression('');

    const nodeData: Partial<HttpNodeData> = {
      typeName,
      displayName: (this.properties.displayName as string | undefined) || 'HTTP',
      description: (this.properties.description as string | undefined) || 'Send HTTP request',
      hasOutput: true,
      outputs: (this.properties.outputs as NodeOutputItem[] | undefined) || [
        { name: 'responseBody', variableType: VariableItemType.StringVariable },
        { name: 'statusCode', variableType: VariableItemType.LongVariable },
      ],
      method: (this.properties.method as HttpMethod | undefined) || 'GET',
      url: (this.properties.url as FullTextMiniExpressionUnit | undefined) || defaultUrl,
      headers: (this.properties.headers as FullTextExpressionUnit | undefined) || defaultHeaders,
      query: (this.properties.query as FullTextExpressionUnit | undefined) || defaultQuery,
      body: (this.properties.body as AnyExpressionUnit | undefined) || defaultBody,
      timeoutSeconds: (this.properties.timeoutSeconds as number | undefined) ?? 5,
    };

    this.height = this.calcHeight();
    this.setNodeData(nodeData);
  }

  setProperties(properties: Record<string, any>) {
    super.setProperties(properties);
    this.height = this.calcHeight();
  }
}

class HttpNodeView extends HtmlNode {
  setHtml(rootEl: SVGForeignObjectElement) {
    const { properties } = this.props.model;
    const data = properties as HttpNodeData;

    const extractText = (unit?: AnyExpressionUnit) => {
      if (!unit) return '';
      if (typeof unit === 'string') return unit;
      if (unit.typeName === 'FullTextMiniExpressionUnit') {
        return (unit as FullTextMiniExpressionUnit).Text || '';
      }
      if (unit.typeName === 'JSExpressionUnit') {
        const js = unit as any;
        return js.isFunctionMode ? js.functionCode : js.expressionCode;
      }
      if (unit.typeName === 'FullTextExpressionUnit') {
        return (unit as any).Text || '';
      }
      return '';
    };

    const urlPreview = extractText(data.url).trim() || 'Set URL';
    const method = data.method || 'GET';

    rootEl.innerHTML = `
      <div class="dify-node http-node">
        <div class="node-header">
          <div class="node-icon" style="background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%); color: #fff;">
            <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" width="18" height="18">
              <path fill="currentColor" d="M715.52 194.752c87.488 0 158.72 71.232 158.72 158.72 0 87.488-71.232 158.72-158.72 158.72H611.84l50.56-50.56h53.12a108.16 108.16 0 0 0 0-216.32H662.4l-50.56-50.56h103.68z m-407.04 377.6c-87.488 0-158.72-71.232-158.72-158.72 0-87.488 71.232-158.72 158.72-158.72h103.68l-50.56 50.56h-53.12a108.16 108.16 0 1 0 0 216.32h53.12l50.56 50.56H308.48z m215.36 60.8l-25.6-25.6 185.6-185.6-185.6-185.6 25.6-25.6 211.2 211.2-211.2 211.2z"/>
            </svg>
          </div>
          <span class="node-name">${data.displayName || 'HTTP'}</span>
        </div>
        <div class="node-info">
          <span class="info-text">${method} ${urlPreview.length > 40 ? urlPreview.slice(0, 40) + '...' : urlPreview}</span>
        </div>
        <div class="node-info">
          <span class="info-text">Timeout: ${data.timeoutSeconds ?? 5}s</span>
        </div>
      </div>
    `;
  }
}

export default {
  type: typeName,
  view: HttpNodeView,
  model: HttpNodeModel,
};