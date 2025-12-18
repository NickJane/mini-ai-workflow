using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.WebApiFramework.Exceptions;

namespace Nop.WebApiFramework
{
    public static class HttpClientExtension
    {
        public static T GetResponse<T>(this JsonResponse<T>? response) where T : class
        {
            if (response != null && response.IsError) throw new WebApiException(errorCode: response.ErrorCode, errorMessage: response.ErrorMessage, requestId: response.RequestId);

            return response?.Data;
        }

        public static void CheckResponseResponse(this JsonResponse? response)
        {
            if (response != null && response.IsError) throw new WebApiException(errorCode: response.ErrorCode, errorMessage: response.ErrorMessage, requestId: response.RequestId);
        }

        public static async Task<T> GetResponseAsync<T>(this HttpResponseMessage response) where T : class
        {
            var requestId = "";

            if (response.Headers.TryGetValues("traceparent", out IEnumerable<string> values))
            {
                requestId = values.FirstOrDefault();
            }

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                var content = await response.Content.ReadAsStringAsync();

                var res = JsonConvert.DeserializeObject<JsonResponse>(content);

                if (res == null) throw new WebApiException((int)response.StatusCode, content, (int)response.StatusCode);

                throw new WebApiException(res);
            }
            string strResp;
            try
            {
                strResp = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new WebApiException(errorMessage: $"请求错误: {ex.Message}");
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(strResp);
            }
            catch (JsonSerializationException e)
            {
                throw new WebApiException(errorMessage: e.Message, requestId: requestId);
            }
        }
    }
}