<template>
  <div class="flow-chat-test">
    <aside class="conversation-sidebar">
      <div class="sidebar-header">
        <div class="sidebar-title">
          <span>{{ t('flowChat.conversations') || '会话' }}</span>
          <el-tag size="small" type="info">{{ sortedConversations.length }}</el-tag>
        </div>
        <div class="sidebar-actions">
          <el-button :icon="Plus" text @click="startNewConversation">{{ t('flowChat.newConversation') || '新建会话' }}</el-button>
          <el-button :icon="Refresh" text :loading="isConversationsLoading" @click="loadConversations(true)">{{ t('flowChat.refresh') || '刷新' }}</el-button>
        </div>
      </div>

      <el-scrollbar class="conversation-list">
        <div
          v-for="(conv, index) in sortedConversations"
          :key="conv.id || conv.title || index"
          :class="['conversation-item', { active: conv.id === conversationId }]"
          @click="selectConversation(conv)"
        >
          <div class="conversation-main">
            <div class="conversation-title">
              <span class="title-text">{{ conv.title || t('flowChat.untitledConversation') || '未命名会话' }}</span>
              <el-tag v-if="conv.isTop" size="small" type="warning">{{ t('flowChat.pin') }}</el-tag>
            </div>
            <div class="conversation-meta">
              <span>{{ formatDateTime(conv.lastMessageTime) }}</span>
              <span class="meta-divider">·</span>
              <span>{{ t('flowChat.messageCount', { count: conv.messageCount || 0 }) }}</span>
            </div>
          </div>
          <div class="conversation-actions" @click.stop>
            <el-button link size="small" @click="toggleConversationTop(conv)">{{ conv.isTop ? t('flowChat.unpin') : t('flowChat.pin') }}</el-button>
            <el-button link size="small" @click="renameConversation(conv)">{{ t('flowChat.rename') }}</el-button>
            <el-button link size="small" type="danger" @click="deleteConversation(conv)">{{ t('flowChat.delete') }}</el-button>
          </div>
        </div>
        <div v-if="!sortedConversations.length && !isConversationsLoading" class="conversation-empty">
          {{ t('flowChat.noConversations') || '暂无会话，点击新建开始聊天' }}
        </div>
        <div v-if="hasMoreConversations" class="conversation-more">
          <el-button text :loading="isConversationsLoading" @click="loadConversations(false)">
            {{ t('flowChat.loadMore') || '加载更多' }}
          </el-button>
        </div>
      </el-scrollbar>
    </aside>

    <section class="chat-panel">
      <!-- 顶部工具栏 -->
      <div class="chat-header">
        <div class="header-left">
          <el-button @click="closeWindow" :icon="Close" text>{{ t('flowChat.close') }}</el-button>
          <el-divider direction="vertical" />
          <span class="chat-title">{{ t('flowChat.title') }}</span>
        </div>
        <div class="header-right">
          <span class="flow-id">{{ t('flowChat.flowId') }}: {{ flowId }}</span>
          <el-button 
            @click="clearConversation" 
            :icon="Delete"
            text
          >
            {{ t('flowChat.clearConversation') }}
          </el-button>
        </div>
      </div>

      <!-- 聊天内容区域 -->
      <div class="chat-content" ref="chatContentRef">
        <div v-if="messagesHasMore" class="load-more">
          <el-button size="small" text :loading="isMessagesLoading" @click="loadMessages(false)">
            {{ t('flowChat.loadMoreMessages') || '加载更多历史消息' }}
          </el-button>
        </div>

        <!-- 欢迎消息 -->
        <div v-if="messages.length === 0" class="welcome-message">
          <div class="welcome-icon">🤖</div>
          <h2>{{ t('flowChat.welcome') }}</h2>
          <p>{{ t('flowChat.welcomeDesc') }}</p>
        </div>

        <!-- 消息列表 -->
        <div v-for="(msg, index) in messages" :key="`msg-${index}-${msg.timestamp}`" class="message-wrapper">
          <!-- 用户消息 -->
          <div v-if="msg.role === 'user'" class="message user-message">
            <div class="message-content">
              <div class="message-text">{{ msg.content }}</div>
              <!-- 文件列表 -->
              <div v-if="msg.files && msg.files.length > 0" class="message-files">
                <div v-for="(file, idx) in msg.files" :key="idx" class="file-item">
                  <el-icon :size="14"><Paperclip /></el-icon>
                  <a :href="file.url" target="_blank" class="file-link">
                    {{ file.name || file.url }}
                  </a>
                </div>
              </div>
            </div>
            <div class="message-avatar">
              <el-icon :size="24"><User /></el-icon>
            </div>
          </div>

          <!-- AI消息 -->
          <div v-else class="message ai-message">
            <div class="message-avatar">
              <el-icon :size="24"><ChatDotRound /></el-icon>
            </div>
            <div class="message-content">
              <!-- 消息文本 -->
              <div class="message-text" v-html="formatMarkdown(msg.content)"></div>
              
              <!-- 运行日志链接 -->
              <div v-if="msg.flowInstanceId" class="message-meta">
                <a 
                  :href="`/flow/logs/${flowId}/${msg.flowInstanceId}`" 
                  target="_blank"
                  class="log-link"
                >
                  {{ t('flowChat.viewRunLog') }}
                </a>
              </div>
              
              <!-- 时间戳 -->
              <div class="message-time">{{ formatTime(msg.timestamp) }}</div>
            </div>
          </div>
        </div>

        <!-- 加载中指示器 -->
        <div v-if="isLoading" class="message ai-message">
          <div class="message-avatar">
            <el-icon :size="24"><ChatDotRound /></el-icon>
          </div>
          <div class="message-content">
            <div class="typing-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        </div>
      </div>

      <!-- 输入区域 -->
      <div class="chat-input-area">
        <!-- 文件列表 -->
        <div v-if="files.length > 0" class="attached-files">
          <div v-for="(file, index) in files" :key="index" class="attached-file">
            <el-icon :size="14"><Paperclip /></el-icon>
            <span class="file-name">{{ file.name || file.url }}</span>
            <el-icon 
              :size="14" 
              class="remove-icon" 
              @click="removeFile(index)"
            >
              <CloseBold />
            </el-icon>
          </div>
        </div>
        
        <div class="input-container">
          <el-input
            ref="inputRef"
            v-model="inputMessage"
            type="textarea"
            :rows="3"
            :placeholder="t('flowChat.inputPlaceholder')"
            :disabled="isLoading"
            @keydown.enter.exact.prevent="sendMessage"
            class="message-input"
          />
          <div class="action-buttons">
            <el-button
              :icon="Paperclip"
              circle
              @click="openFileDialog"
              :disabled="isLoading"
              class="attach-btn"
            />
            <el-button
              type="primary"
              :icon="Promotion"
              :loading="isLoading"
              :disabled="isLoading || !inputMessage.trim()"
              @click="sendMessage"
              class="send-btn"
            >
              {{ t('flowChat.send') }}
            </el-button>
          </div>
        </div>
        
        <!-- 会话信息 -->
        <div class="conversation-info">
          <el-tag v-if="conversationId" size="small" type="success">
            {{ t('flowChat.conversationId') }}: {{ conversationId }}
          </el-tag>
          <el-tag v-if="currentFlowInstanceId" size="small" type="warning">
            {{ t('flowChat.flowInstanceId') }}: {{ currentFlowInstanceId }}
          </el-tag>
          <el-tag v-if="!conversationId" size="small" type="info">
            {{ t('flowChat.newConversationTip') || '新会话，发送后自动创建' }}
          </el-tag>
        </div>
      </div>

      <!-- 添加文件对话框 -->
      <el-dialog
        v-model="fileDialogVisible"
        :title="t('flowChat.addFile')"
        width="500px"
      >
        <el-form label-width="80px">
          <el-form-item :label="t('flowChat.fileUrl')" required>
            <el-input
              v-model="fileUrl"
              :placeholder="t('flowChat.fileUrlPlaceholder')"
              @keydown.enter.prevent="addFile"
            />
          </el-form-item>
          <el-form-item :label="t('flowChat.fileName')">
            <el-input
              v-model="fileName"
              :placeholder="t('flowChat.fileNamePlaceholder')"
              @keydown.enter.prevent="addFile"
            />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="fileDialogVisible = false">{{ t('flowChat.cancel') }}</el-button>
          <el-button type="primary" @click="addFile">{{ t('flowChat.add') }}</el-button>
        </template>
      </el-dialog>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onBeforeUnmount, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { Close, Delete, User, ChatDotRound, Promotion, Paperclip, Plus, CloseBold, Refresh } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { marked } from 'marked';
import { FlowConversationService } from '@/services/flowConversation.service';
import type { LocalConversation, LocalMessage } from '@/types/flowConversation.types';
import type { AIFileRequest } from '@/types/flow.types';

// 定义文件类型
interface FileItem {
  url: string;
  name?: string;
  size?: number;
  mimeType?: string;
}

// 定义消息类型
interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: number;
  flowInstanceId?: number;  // 流程实例ID，用于查看运行日志
  files?: FileItem[];  // 附件文件列表
}

// 流式响应事件类型
interface StreamEvent {
  event: 'workflow_started' | 'message' | 'workflow_finished';
  data?: {
    conversationId?: string;
    flowInstanceId?: number;
    createdAt?: number;
  };
  answer?: string;
  createdAt?: number;
  metadata?: {
    node_id?: string;
  };
}

const { t } = useI18n();
const route = useRoute();
const flowId = computed(() => route.params.flowId as string);

// 状态管理
const messages = ref<ChatMessage[]>([]);
const inputMessage = ref('');
const isLoading = ref(false);
const isMessagesLoading = ref(false);
const conversationId = ref<string>('');
const currentFlowInstanceId = ref<number | null>(null);
const chatContentRef = ref<HTMLElement | null>(null);
const inputRef = ref<any>(null);  // 输入框引用

// 文件管理
const files = ref<FileItem[]>([]);
const fileDialogVisible = ref(false);
const fileUrl = ref('');
const fileName = ref('');

// 会话管理
const conversations = ref<LocalConversation[]>([]);
const isConversationsLoading = ref(false);
const hasMoreConversations = ref(false);
const conversationCursor = ref<string | undefined>(undefined);

// 消息分页
const messagesHasMore = ref(false);
const messageCursor = ref<string | undefined>(undefined);
const messagePageSize = 20;

// 当前正在接收的AI消息
const currentAIMessage = ref<ChatMessage | null>(null);

// 排序后的会话列表
const sortedConversations = computed(() => {
  return [...conversations.value].sort((a, b) => {
    if (a.isTop !== b.isTop) return a.isTop ? -1 : 1;
    const aTime = a.lastMessageTime ? new Date(a.lastMessageTime).getTime() : 0;
    const bTime = b.lastMessageTime ? new Date(b.lastMessageTime).getTime() : 0;
    return bTime - aTime;
  });
});

// 获取用户手机号
const getUserPhone = (): string => {
  return localStorage.getItem('phoneNumber') || 'anonymous';
};

// 构造通用请求头（包含鉴权/语言）
const getCommonHeaders = () => {
  const tokenInfo = JSON.parse(localStorage.getItem('tokenInfo') || 'null');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };
  if (tokenInfo?.type && tokenInfo?.accessToken) {
    headers.Authorization = `${tokenInfo.type} ${tokenInfo.accessToken}`;
  }
  const phone = localStorage.getItem('phoneNumber');
  if (phone) headers['phoneNumber'] = phone;
  const language = localStorage.getItem('lang') || 'zh-CN';
  headers['Accept-Language'] = language;
  headers['X-Language'] = language;
  return headers;
};

// 格式化时间
const formatTime = (timestamp: number): string => {
  const date = new Date(timestamp);
  return date.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' });
};

const formatDateTime = (iso?: string | null) => {
  if (!iso) return '';
  const d = new Date(iso);
  return d.toLocaleString();
};

// 格式化Markdown，并处理深度思考标签（支持流式渲染不完整标签）
const formatMarkdown = (text: string): string => {
  if (!text) return '';
  
  let result = '';
  let remaining = text;
  
  // 循环处理所有 <think> 标签
  while (remaining.length > 0) {
    const thinkStart = remaining.indexOf('<think>');
    
    if (thinkStart === -1) {
      // 没有 <think> 标签，直接 Markdown 处理剩余内容
      result += marked(remaining, { breaks: true }) as string;
      break;
    }
    
    // 处理 <think> 之前的普通文本
    if (thinkStart > 0) {
      result += marked(remaining.substring(0, thinkStart), { breaks: true }) as string;
    }
    
    // 查找对应的 </think>
    const thinkEnd = remaining.indexOf('</think>', thinkStart);
    
    if (thinkEnd === -1) {
      // 未闭合的 <think> 标签（流式渲染中）
      const thinkContent = remaining.substring(thinkStart + 7); // 7 = '<think>'.length
      result += `<div class="thinking-block thinking-streaming">
        <div class="thinking-header">
          <span class="thinking-icon">🧠</span>
          <span class="thinking-title">${t('flowChat.thinkingProcess')}</span>
          <span class="thinking-indicator">...</span>
        </div>
        <div class="thinking-content">${thinkContent}</div>
      </div>`;
      break;
    } else {
      // 完整的 <think>...</think> 标签
      const thinkContent = remaining.substring(thinkStart + 7, thinkEnd);
      result += `<div class="thinking-block">
        <div class="thinking-header">
          <span class="thinking-icon">🧠</span>
          <span class="thinking-title">${t('flowChat.thinkingProcess')}</span>
        </div>
        <div class="thinking-content">${thinkContent.trim()}</div>
      </div>`;
      
      // 继续处理 </think> 之后的内容
      remaining = remaining.substring(thinkEnd + 8); // 8 = '</think>'.length
    }
  }
  
  return result;
};

// 滚动到底部
const scrollToBottom = () => {
  nextTick(() => {
    if (chatContentRef.value) {
      chatContentRef.value.scrollTop = chatContentRef.value.scrollHeight;
    }
  });
};

// 添加文件
const addFile = () => {
  if (!fileUrl.value.trim()) {
    ElMessage.warning(t('flowChat.fileUrlRequired'));
    return;
  }
  
  files.value.push({
    url: fileUrl.value.trim(),
    name: fileName.value.trim() || undefined
  });
  
  fileUrl.value = '';
  fileName.value = '';
  fileDialogVisible.value = false;
  ElMessage.success(t('flowChat.fileAdded'));
};

// 删除文件
const removeFile = (index: number) => {
  files.value.splice(index, 1);
  ElMessage.success(t('flowChat.fileDeleted'));
};

// 打开文件对话框
const openFileDialog = () => {
  fileDialogVisible.value = true;
};

// 发送消息
const sendMessage = async () => {
  if (!inputMessage.value.trim() || isLoading.value) return;

  const userMessage = reactive<ChatMessage>({
    role: 'user',
    content: inputMessage.value.trim(),
    timestamp: Date.now(),
    files: files.value.length > 0 ? [...files.value] : undefined
  });

  messages.value.push(userMessage);
  const query = inputMessage.value.trim();
  const messageFiles = [...files.value];
  inputMessage.value = '';
  files.value = [];  // 清空文件列表
  isLoading.value = true;
  scrollToBottom();

  try {
    // 准备请求数据
    const requestBody = {
      query: query,
      user: getUserPhone(),
      conversationId: conversationId.value || '',
      files: messageFiles
    };

    const apiBase = import.meta.env.VITE_API_BASE_URL || '';
    // 发起流式请求
    const response = await fetch(`${apiBase}/Flow/chat-messages/${flowId.value}`, {
      method: 'POST',
      headers: getCommonHeaders(),
      body: JSON.stringify(requestBody)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // 读取流式响应
    const reader = response.body?.getReader();
    const decoder = new TextDecoder();

    if (!reader) {
    throw new Error(t('flowChat.noResponse'));
    }

    // 创建AI消息（使用 reactive 包装，确保内容更新时能触发 v-html 重新计算）
    const aiMessage = reactive<ChatMessage>({
      role: 'assistant',
      content: '',
      timestamp: Date.now()
    });
    messages.value.push(aiMessage);
    currentAIMessage.value = aiMessage;

    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();
      
      if (done) break;

      // 解码数据
      buffer += decoder.decode(value, { stream: true });
      
      // 处理完整的事件行
      const lines = buffer.split('\n');
      buffer = lines.pop() || ''; // 保留不完整的行

      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = line.substring(6).trim();
          
          if (data === '[DONE]') {
            break;
          }

          try {
            const event: StreamEvent = JSON.parse(data);
            handleStreamEvent(event);
          } catch (e) {
            console.error('解析事件失败:', e, data);
          }
        }
      }

      scrollToBottom();
    }

    // 完成后的处理
    currentAIMessage.value = null;
    await loadConversations(true);
    
  } catch (error) {
    console.error('Failed to send message:', error);
    ElMessage.error(t('flowChat.sendFailed'));
    
    // 移除未完成的AI消息
    if (currentAIMessage.value) {
      const index = messages.value.indexOf(currentAIMessage.value);
      if (index > -1) {
        messages.value.splice(index, 1);
      }
      currentAIMessage.value = null;
    }
  } finally {
    isLoading.value = false;
    
    // 响应完成后自动聚焦到输入框
    nextTick(() => {
      if (inputRef.value) {
        inputRef.value.focus();
      }
    });
  }
};

// 处理流式事件
const handleStreamEvent = (event: StreamEvent) => {
  switch (event.event) {
    case 'workflow_started':
      // 工作流开始
      if (event.data?.conversationId) {
        conversationId.value = event.data.conversationId;
      }
      if (event.data?.flowInstanceId) {
        currentFlowInstanceId.value = event.data.flowInstanceId;
      }
      break;

    case 'message':
      // 接收消息片段
      if (currentAIMessage.value && event.answer) {
        // 直接更新内容，Vue 3 会自动追踪
        currentAIMessage.value.content += event.answer;
        
        // 设置流程实例ID（用于查看运行日志）
        if (currentFlowInstanceId.value && !currentAIMessage.value.flowInstanceId) {
          currentAIMessage.value.flowInstanceId = currentFlowInstanceId.value;
        }
      }
      break;

    case 'workflow_finished':
      // 工作流结束
      break;
  }
};

const mapLocalMessageToChatMessages = (msg: LocalMessage): ChatMessage[] => {
  const createdTime = msg.createdAt ? new Date(msg.createdAt).getTime() : Date.now();
  const chats: ChatMessage[] = [];
  if (msg.question) {
    chats.push({
      role: 'user',
      content: msg.question,
      timestamp: createdTime,
      files: (msg.files as AIFileRequest[] | undefined) || undefined
    });
  }
  if (msg.answer) {
    chats.push({
      role: 'assistant',
      content: msg.answer,
      timestamp: createdTime
    });
  }
  return chats;
};

const loadMessages = async (reset = true) => {
  if (!conversationId.value) return;
  if (isMessagesLoading.value) return;
  isMessagesLoading.value = true;

  try {
    const res = await FlowConversationService.getLocalMessages(flowId.value, {
      conversation_id: conversationId.value,
      first_id: reset ? undefined : messageCursor.value,
      limit: messagePageSize
    });

    const payload = res.data || [];
    const mapped = payload.flatMap(mapLocalMessageToChatMessages).sort((a, b) => a.timestamp - b.timestamp);

    if (reset) {
      messages.value = mapped;
    } else {
      messages.value = [...mapped, ...messages.value];
    }

    const sourceList = res.data || [];
    if (sourceList.length > 0) {
      messageCursor.value = sourceList[sourceList.length - 1].id || undefined;
    }
    messagesHasMore.value = res.hasMore ?? false;
    scrollToBottom();
  } catch (error) {
    console.error('加载消息失败', error);
    ElMessage.error(t('flowChat.loadMessagesFailed'));
  } finally {
    isMessagesLoading.value = false;
  }
};

const loadConversations = async (reset = true) => {
  if (isConversationsLoading.value) return;
  isConversationsLoading.value = true;

  try {
    const res = await FlowConversationService.getLocalConversations(flowId.value, {
      first_id: reset ? undefined : conversationCursor.value
    });

    const list = res.data || [];
    if (reset) {
      conversations.value = list;
    } else {
      conversations.value = [...conversations.value, ...list];
    }
    hasMoreConversations.value = res.hasMore ?? false;
    if (list.length > 0) {
      conversationCursor.value = list[list.length - 1].id || undefined;
    }

    // 初次加载时，自动选中第一条会话
    if (reset && list.length > 0 && !conversationId.value) {
      conversationId.value = list[0].id || '';
      await loadMessages(true);
    }
  } catch (error) {
    console.error('加载会话失败', error);
    ElMessage.error(t('flowChat.loadConversationsFailed'));
  } finally {
    isConversationsLoading.value = false;
  }
};

const selectConversation = async (conv: LocalConversation) => {
  if (!conv.id) return;
  conversationId.value = conv.id;
  currentFlowInstanceId.value = null;
  messageCursor.value = undefined;
  messagesHasMore.value = false;
  await loadMessages(true);
};

const startNewConversation = () => {
  conversationId.value = '';
  messages.value = [];
  currentAIMessage.value = null;
  currentFlowInstanceId.value = null;
  messageCursor.value = undefined;
  messagesHasMore.value = false;
};

const toggleConversationTop = async (conv: LocalConversation) => {
  if (!conv.id) return;
  try {
    await FlowConversationService.toggleConversationTop(flowId.value, conv.id);
    await loadConversations(true);
  } catch (error) {
    console.error('置顶失败', error);
    ElMessage.error(t('flowChat.operationFailed'));
  }
};

const renameConversation = async (conv: LocalConversation) => {
  if (!conv.id) return;
  try {
    const { value } = await ElMessageBox.prompt(t('flowChat.renamePrompt'), t('flowChat.renameConversation'), {
      inputValue: conv.title || '',
      confirmButtonText: t('flowChat.confirm'),
      cancelButtonText: t('flowChat.cancel')
    });
    await FlowConversationService.updateConversationTitle(flowId.value, conv.id, value);
    await loadConversations(true);
  } catch (error) {
    if (error !== 'cancel') {
      console.error('重命名失败', error);
      ElMessage.error(t('flowChat.renameFailed'));
    }
  }
};

const deleteConversation = async (conv: LocalConversation) => {
  if (!conv.id) return;
  try {
    await ElMessageBox.confirm(t('flowChat.deleteConfirm'), t('flowChat.deleteConversation'), {
      confirmButtonText: t('flowChat.delete'),
      cancelButtonText: t('flowChat.cancel'),
      type: 'warning'
    });
    await FlowConversationService.deleteConversation(flowId.value, conv.id);
    if (conversationId.value === conv.id) {
      startNewConversation();
    }
    await loadConversations(true);
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除失败', error);
      ElMessage.error(t('flowChat.deleteFailed'));
    }
  }
};

// 清空对话
const clearConversation = () => {
  messages.value = [];
  conversationId.value = '';
  currentFlowInstanceId.value = null;
  currentAIMessage.value = null;
  messageCursor.value = undefined;
  messagesHasMore.value = false;
  ElMessage.success(t('flowChat.conversationCleared'));
  
  // 清空后自动聚焦到输入框
  nextTick(() => {
    if (inputRef.value) {
      inputRef.value.focus();
    }
  });
};

// 关闭窗口
const closeWindow = () => {
  window.close();
};

onMounted(async () => {
  scrollToBottom();
  
  // 页面加载完成后自动聚焦到输入框
  nextTick(() => {
    if (inputRef.value) {
      inputRef.value.focus();
    }
  });

  await loadConversations(true);
});

onBeforeUnmount(() => {
  // 预留：组件卸载时的清理
});
</script>

<style scoped>
.flow-chat-test {
  width: 100vw;
  height: 100vh;
  display: flex;
  flex-direction: row;
  background: linear-gradient(135deg, #eef2ff 0%, #e0e7ff 100%);
  overflow: hidden;
}

.conversation-sidebar {
  width: 320px;
  background: #f8fafc;
  border-right: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  padding: 16px;
  gap: 12px;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.sidebar-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #1f2937;
}

.sidebar-actions {
  display: flex;
  gap: 4px;
}

.conversation-list {
  flex: 1;
}

.conversation-item {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 12px;
  margin-bottom: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.conversation-item:hover {
  border-color: #c7d2fe;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.08);
}

.conversation-item.active {
  border-color: #6366f1;
  box-shadow: 0 6px 16px rgba(99, 102, 241, 0.15);
}

.conversation-main {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.conversation-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #111827;
}

.title-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.conversation-meta {
  font-size: 12px;
  color: #6b7280;
  display: flex;
  align-items: center;
  gap: 6px;
}

.meta-divider {
  color: #d1d5db;
}

.conversation-actions {
  margin-top: 8px;
  display: flex;
  gap: 6px;
}

.conversation-empty {
  text-align: center;
  color: #9ca3af;
  padding: 24px 0;
}

.conversation-more {
  text-align: center;
  padding: 8px 0 16px;
}

.chat-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
}

/* 顶部工具栏 */
.chat-header {
  height: 60px;
  padding: 0 24px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.header-left,
.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.chat-title {
  font-size: 18px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.flow-id {
  font-size: 13px;
  color: #6b7280;
}

/* 聊天内容区域 */
.chat-content {
  flex: 1;
  padding: 16px 24px;
  overflow-y: auto;
  background: rgba(255, 255, 255, 0.9);
  margin: 16px;
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.load-more {
  display: flex;
  justify-content: center;
  margin-bottom: 8px;
}

/* 欢迎消息 */
.welcome-message {
  text-align: center;
  padding: 60px 20px;
  color: #6b7280;
}

.welcome-icon {
  font-size: 64px;
  margin-bottom: 16px;
}

.welcome-message h2 {
  font-size: 24px;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 8px;
}

.welcome-message p {
  font-size: 14px;
  color: #9ca3af;
}

/* 消息容器 */
.message-wrapper {
  margin-bottom: 24px;
}

.message {
  display: flex;
  gap: 12px;
  animation: messageSlideIn 0.3s ease-out;
}

@keyframes messageSlideIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 用户消息 */
.user-message {
  justify-content: flex-end;
}

.user-message .message-content {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 18px 18px 4px 18px;
  padding: 12px 18px;
  max-width: 70%;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
}

.user-message .message-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* AI消息 */
.ai-message {
  justify-content: flex-start;
}

.ai-message .message-content {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 18px 18px 18px 4px;
  padding: 12px 18px;
  max-width: 70%;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.ai-message .message-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* 消息文本 */
.message-text {
  font-size: 14px;
  line-height: 1.6;
  word-wrap: break-word;
  white-space: pre-wrap;
}

.user-message .message-text {
  color: white;
}

.ai-message .message-text {
  color: #1f2937;
}

/* Markdown样式 */
.message-text :deep(p) {
  margin: 0;
  padding: 0;
}

.message-text :deep(pre) {
  background: #f3f4f6;
  padding: 8px;
  border-radius: 4px;
  overflow-x: auto;
  margin: 8px 0;
}

.message-text :deep(code) {
  background: #f3f4f6;
  padding: 2px 4px;
  border-radius: 3px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
}

/* 深度思考块样式 - 优雅的浅色主题 */
.message-text :deep(.thinking-block) {
  margin: 12px 0;
  border-radius: 10px;
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border: 1.5px solid #7dd3fc;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(56, 189, 248, 0.15);
  transition: all 0.3s ease;
}

.message-text :deep(.thinking-block):hover {
  box-shadow: 0 4px 12px rgba(56, 189, 248, 0.25);
  border-color: #38bdf8;
}

.message-text :deep(.thinking-header) {
  padding: 10px 14px;
  background: linear-gradient(135deg, rgba(56, 189, 248, 0.08) 0%, rgba(14, 165, 233, 0.08) 100%);
  border-bottom: 1px solid rgba(56, 189, 248, 0.2);
  display: flex;
  align-items: center;
  gap: 8px;
}

.message-text :deep(.thinking-icon) {
  font-size: 18px;
  line-height: 1;
}

.message-text :deep(.thinking-title) {
  font-size: 12px;
  font-weight: 600;
  color: #0369a1;
  letter-spacing: 0.3px;
  flex: 1;
}

.message-text :deep(.thinking-indicator) {
  font-size: 12px;
  color: #0ea5e9;
  animation: thinkingPulse 1.5s ease-in-out infinite;
}

@keyframes thinkingPulse {
  0%, 100% {
    opacity: 0.4;
  }
  50% {
    opacity: 1;
  }
}

.message-text :deep(.thinking-content) {
  padding: 14px;
  color: #0c4a6e;
  font-size: 13px;
  line-height: 1.7;
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  background: rgba(255, 255, 255, 0.5);
}

/* 流式渲染中的动画效果 */
.message-text :deep(.thinking-streaming) {
  animation: thinkingGlow 2s ease-in-out infinite;
}

@keyframes thinkingGlow {
  0%, 100% {
    border-color: #7dd3fc;
  }
  50% {
    border-color: #38bdf8;
  }
}

/* 消息元信息 */
.message-meta {
  margin-top: 8px;
  display: flex;
  gap: 8px;
}

/* 日志链接 */
.log-link {
  font-size: 12px;
  color: #667eea;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  transition: all 0.2s ease;
  font-weight: 500;
}

.log-link:hover {
  color: #764ba2;
  text-decoration: underline;
}

.log-link::before {
  content: '📋';
  font-size: 14px;
}

.message-time {
  font-size: 12px;
  color: #9ca3af;
  margin-top: 4px;
}

/* 加载动画 */
.typing-indicator {
  display: flex;
  gap: 4px;
  padding: 8px 0;
}

.typing-indicator span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #9ca3af;
  animation: typing 1.4s infinite;
}

.typing-indicator span:nth-child(2) {
  animation-delay: 0.2s;
}

.typing-indicator span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes typing {
  0%, 60%, 100% {
    transform: translateY(0);
    opacity: 0.5;
  }
  30% {
    transform: translateY(-10px);
    opacity: 1;
  }
}

/* 输入区域 */
.chat-input-area {
  padding: 16px 24px 24px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-top: 1px solid rgba(0, 0, 0, 0.05);
  flex-shrink: 0;
}

.input-container {
  display: flex;
  gap: 12px;
  align-items: flex-end;
}

.message-input {
  flex: 1;
}

.action-buttons {
  display: flex;
  gap: 8px;
  align-items: flex-end;
}

.attach-btn {
  width: 40px;
  height: 40px;
  border: 2px solid #e5e7eb;
  color: #667eea;
  transition: all 0.3s ease;
}

.attach-btn:hover {
  border-color: #667eea;
  background: #f0f4ff;
}

/* 附加文件列表 */
.attached-files {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}

.attached-file {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #f3f4f6;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 13px;
  color: #4b5563;
  transition: all 0.2s ease;
}

.attached-file:hover {
  background: #e5e7eb;
}

.attached-file .file-name {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.attached-file .remove-icon {
  cursor: pointer;
  color: #9ca3af;
  transition: color 0.2s ease;
}

.attached-file .remove-icon:hover {
  color: #ef4444;
}

/* 用户消息中的文件列表 */
.message-files {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.message-files .file-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  opacity: 0.9;
}

.message-files .file-link {
  color: inherit;
  text-decoration: underline;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message-files .file-link:hover {
  opacity: 0.8;
}

.message-input :deep(.el-textarea__inner) {
  border-radius: 12px;
  border: 2px solid #e5e7eb;
  transition: all 0.3s ease;
  font-size: 14px;
  resize: none;
}

.message-input :deep(.el-textarea__inner:focus) {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.send-button {
  height: 40px;
  padding: 0 24px;
  border-radius: 20px;
  font-weight: 500;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  transition: all 0.3s ease;
}

.send-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.send-button:active {
  transform: translateY(0);
}

/* 会话信息 */
.conversation-info {
  margin-top: 12px;
  display: flex;
  gap: 8px;
  font-size: 12px;
}

/* 滚动条样式 */
.chat-content::-webkit-scrollbar {
  width: 6px;
}

.chat-content::-webkit-scrollbar-track {
  background: transparent;
}

.chat-content::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 3px;
}

.chat-content::-webkit-scrollbar-thumb:hover {
  background: #9ca3af;
}
</style>
