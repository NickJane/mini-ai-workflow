using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Nop.WebApiFramework
{

    public class GroupInfoAttribute : Attribute
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }

    public class ApiGroupAttribute : Attribute, IApiDescriptionGroupNameProvider
    {

        public ApiGroupAttribute(string name)
        {
            GroupName = name.ToString();
        }

        public string GroupName { get; set; }
    }
}
