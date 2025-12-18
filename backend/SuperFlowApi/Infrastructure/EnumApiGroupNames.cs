


using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Nop.WebApiFramework;

namespace SuperFlowApi.Infrastructure
{
    /// <summary>
    /// 接口分组枚举
    /// </summary>
    public enum EnumApiGroupNames
    {
        [GroupInfo(Title = "账户相关", Description = "账户相关", Version = "v1")]
        UserAccount,

        [GroupInfo(Title = "流程管理", Description = "流程管理", Version = "v1")]
        SuperFlow,
    }
}