using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.WebApiFramework.Exceptions
{
    public class WebApiException : Exception
    {
        /// <summary>
        /// 自定义业务公共异常
        /// </summary>
        /// <param name="errorMessage"></param>
        public WebApiException(string errorMessage) : base(errorMessage)
        {
            ErrorCode = 1;
            ErrorMessage = errorMessage;
            HttpStatus = 200;
        }

        /// <summary>
        /// 自定义业务公共异常
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="httpStatus"></param>
        /// <param name="requestId"></param>
        /// <param name="responseData"></param>
        public WebApiException(int errorCode = 1, string errorMessage = "error", int httpStatus = 200, string? requestId = null, object? responseData = null) : base(errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            HttpStatus = httpStatus;
            RequestId = requestId;
            ResponseData = responseData;
        }

        /// <summary>
        /// 自定义业务公共异常
        /// </summary>
        /// <param name="response"></param>
        /// <param name="httpStatus"></param>
        /// <param name="responseData"></param>
        public WebApiException(JsonResponse response, int httpStatus = 200, object? responseData = null) : base(response.ErrorMessage)
        {
            ErrorCode = response.ErrorCode;
            ErrorMessage = response.ErrorMessage;
            HttpStatus = httpStatus;
            RequestId = response.RequestId;
            ResponseData = responseData;
        }

        /// <summary>
        /// 错误内容
        /// </summary>
        public string ErrorMessage { get; set; } = null!;
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 自定义返回状态
        /// </summary>
        public int HttpStatus { get; set; }
        /// <summary>
        /// 返回的数据
        /// </summary>
        public object? ResponseData { get; set; }
        /// <summary>
        /// 请求Id
        /// </summary>
        public string? RequestId { get; set; }
    }
}
