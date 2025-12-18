using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Nop.WebApiFramework.ServiceExtentions
{

    public static class SwaggerExtentions
    {

        /// <summary>
        /// 添加 API 版本控制扩展方法
        /// </summary>
        /// <param name="services">生命周期中注入的服务集合 <see cref="IServiceCollection"/></param>
        public static void AddCustomSwagger(this IServiceCollection services, Type enumApiGroupNames = null)
        {
            services.AddSwaggerGen(options =>
            {
                options.SchemaFilter<NewtonsoftEnumSchemaFilter>();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "接口文档",
                    Version = "v1",
                    Description = "测试 webapi"
                });

                // 遍历ApiGroupNames所有枚举值生成接口文档，Skip(1)是因为Enum第一个FieldInfo是内置的一个Int值
                enumApiGroupNames?.GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //获取枚举值上的特性
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    options.SwaggerDoc(f.Name, new OpenApiInfo
                    {
                        Title = info?.Title,
                        Version = info?.Version,
                        Description = info?.Description
                    });
                });
                // 没有特性的接口分到NoGroup上
                options.SwaggerDoc("NoGroup", new OpenApiInfo
                {
                    Title = "无分组"
                });
                // 判断接口归于哪个分组
                options.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (docName == "NoGroup")
                    {
                        // 当分组为NoGroup时，只要没加特性的接口都属于这个组
                        return string.IsNullOrEmpty(apiDescription.GroupName);
                    }
                    else
                    {
                        return apiDescription.GroupName == docName;
                    }
                });

                #region 读取xml信息
                //option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml"), true);
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var folders = new DirectoryInfo(basePath);
                foreach (var file in folders.GetFiles("*.xml"))
                {
                    options.IncludeXmlComments(file.FullName, true);
                }
                #endregion

                #region 启用swagger验证功能
                // 启用swagger的输入token的功能
                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "请输入Bearer {token}",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] { }
                    }
                });
                #endregion

            });

            services.AddSwaggerGenNewtonsoftSupport();
        }


        public static void UseCustomSwagger(this WebApplication app, Type apiGroup = null)
        {
            //启用Swagger中间件 
            app.UseSwagger();
            // 配置SwaggerUI
            app.UseSwaggerUI(c =>
            {
                // 折叠所有的Tag
                c.DocExpansion(DocExpansion.None);
                // 隐藏API中定义的model
                c.DefaultModelsExpandDepth(-1);
                // apiGroup.GetFields() 枚举的 GetFields第二个值开始才是枚举内的自定义字段
                apiGroup?.GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //获取枚举值上的特性
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    c.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json", info != null ? info.Title : f.Name);
                });
                c.SwaggerEndpoint("/swagger/NoGroup/swagger.json", "无分组");
                //c.RoutePrefix = string.Empty; 这个属性决定访问swagger的地址, 注释掉的话, 就是 http://localhost:19001/index.html
            });
        }
    }

    public class NewtonsoftEnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (type.IsEnum)
            {
                schema.Type = "string";
                schema.Format = null;
                schema.Enum.Clear();

                var names = Enum.GetNames(type);
                foreach (var name in names)
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
                }
            }
        }
    }
}
