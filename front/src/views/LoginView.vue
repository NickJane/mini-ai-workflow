<!--
  登录页面
  提供用户登录/注册功能
 -->
<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <h2 class="login-title">System Login</h2>
      </template>
      <el-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        @submit.prevent="handleSubmit"
      >
        <!-- 手机号输入框 -->
        <el-form-item prop="phoneNumber">
          <el-input
            v-model="formData.phoneNumber"
            placeholder="Enter phone number"
            prefix-icon="User"
          />
        </el-form-item>
        
        <!-- 密码输入框 -->
        <el-form-item prop="password">
          <el-input
            v-model="formData.password"
            type="password"
            placeholder="Enter password"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        
        <!-- 提交按钮 -->
        <el-form-item>
          <el-button
            type="primary"
            native-type="submit"
            :loading="loading"
            class="submit-btn"
          >
            {{ loading ? "Logging in..." : "Register / Login" }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import { withLoading } from "@/utils/decorator.util";
import type { FormInstance } from "element-plus";

const router = useRouter();
const authStore = useAuthStore();
const formRef = ref<FormInstance>();
const loading = ref(false);

// 表单数据
const formData = reactive({
  phoneNumber: "",
  password: "",
});

// 表单验证规则
const rules = {
  phoneNumber: [
    { required: true, message: "Please enter phone number", trigger: "blur" },
    {
      pattern: /^1[3-9]\d{9}$/,
      message: "Please enter a valid phone number",
      trigger: "blur",
    },
  ],
  password: [
    { required: true, message: "Please enter password", trigger: "blur" },
    { min: 6, message: "Password must be at least 6 characters", trigger: "blur" },
  ],
};

// 提交处理
const handleSubmit = async () => {
  if (!formRef.value) return;

  await formRef.value.validate(async (valid) => {
    if (valid) {
      await withLoading(
        async () => {
          const response = await authStore.signIn(
            formData.phoneNumber,
            formData.password
          );
          router.push("/");
          return response;
        },
        loading,
        "Login successful"
      );
    }
  });
};
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #f5f7fa;
}

.login-card {
  width: 100%;
  max-width: 400px;
}

.login-title {
  text-align: center;
  margin: 0;
  font-size: 24px;
  color: #303133;
}

.submit-btn {
  width: 100%;
}
</style>
