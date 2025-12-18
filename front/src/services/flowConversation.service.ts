import type { TApiResponse } from '@/types/ApiResponse{T}';
import type { LocalConversationsResponse, LocalMessagesResponse } from '@/types/flowConversation.types';
import { HttpUtil } from '@/utils/http.util';

export class FlowConversationService {
  public static async getLocalConversations(
    flowId: string,
    params?: { first_id?: string; limit?: number }
  ): Promise<LocalConversationsResponse> {
    const response = await HttpUtil.getInstance().get<TApiResponse<LocalConversationsResponse>>(
      `/FlowConversation/${flowId}/local-conversations`,
      {
        params: {
          first_id: params?.first_id,
          limit: params?.limit ?? 20
        }
      }
    );
    return (
      response.data.data ?? {
        data: [],
        hasMore: false,
        limit: params?.limit ?? 20
      }
    );
  }

  public static async getLocalMessages(
    flowId: string,
    params: { conversation_id: string; first_id?: string; limit?: number }
  ): Promise<LocalMessagesResponse> {
    const response = await HttpUtil.getInstance().get<TApiResponse<LocalMessagesResponse>>(
      `/FlowConversation/${flowId}/local-messages`,
      {
        params: {
          conversation_id: params.conversation_id,
          first_id: params.first_id,
          limit: params.limit ?? 20
        }
      }
    );
    return (
      response.data.data ?? {
        data: [],
        hasMore: false,
        limit: params.limit ?? 20
      }
    );
  }

  public static async toggleConversationTop(flowId: string, conversationId: string) {
    await HttpUtil.getInstance().post<TApiResponse<boolean>>(
      `/FlowConversation/${flowId}/local-conversations-toggle-top`,
      null,
      { params: { conversationId } }
    );
  }

  public static async updateConversationTitle(flowId: string, conversationId: string, title: string) {
    await HttpUtil.getInstance().post<TApiResponse<boolean>>(
      `/FlowConversation/${flowId}/local-conversations-update-title`,
      { title },
      { params: { conversationId } }
    );
  }

  public static async deleteConversation(flowId: string, conversationId: string) {
    await HttpUtil.getInstance().post<TApiResponse<boolean>>(
      `/FlowConversation/${flowId}/local-conversations-delete`,
      null,
      { params: { conversationId } }
    );
  }
}

