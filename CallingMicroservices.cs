using FluentValidation.Results;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProFinance.Mvc.Enums;
using ProFinance.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Example
{
    public class CallingMicroservices
    {
        public readonly HttpClient client;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public CallingMicroservices(HttpClient client, Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor, IConfiguration configuration)
        {
            this.client = client;
            this._httpContextAccessor = _httpContextAccessor;
            this._configuration = configuration;
        }


        #region Call Microservices
        public async Task<string> GetAsyncString(string url)
        {
            if (Convert.ToBoolean(_configuration["IdentityServer:UseIdentityServer"]))
            {
                client.SetBearerToken(_httpContextAccessor?.HttpContext);
                var oid = Guid.NewGuid().ToString();
                client.DefaultRequestHeaders.Remove("oid");
                client.DefaultRequestHeaders.Add("oid", oid);
                var userString = _httpContextAccessor?.HttpContext?.Session?.GetString(SessionType.User.ToString());
                if (!string.IsNullOrEmpty(userString))
                {
                    var user = JsonConvert.DeserializeObject<Domain.SessionUser>(userString);
                    client.DefaultRequestHeaders.Remove("httpUser");
                    client.DefaultRequestHeaders.Add("httpUser", user.Username);
                }
                else
                {
                    var cp = _httpContextAccessor?.HttpContext?.User;
                    var username = cp?.FindFirst("name")?.Value;
                    client.DefaultRequestHeaders.Remove("httpUser");
                    client.DefaultRequestHeaders.Add("httpUser", username);
                }
            }

            var response = client.GetAsync(url);
            var msg = await response;

            switch (msg.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var res = response.Result.Content.ReadAsStringAsync();
                    return await res;
                case System.Net.HttpStatusCode.BadRequest:
                    var content = msg.ReasonPhrase;
                    throw new ValidationException(content);
                case System.Net.HttpStatusCode.NotFound:
                    try
                    {
                        var NotFoundRes = response.Result.Content.ReadAsStringAsync();
                        var notfoundJSON = JsonConvert.DeserializeObject<NotFoundModel>(NotFoundRes.Result);
                        return await NotFoundRes;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(msg.ReasonPhrase);
                    }
                    
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException();
            }

            throw new Exception(response.Result.StatusCode.ToString());
        }
       

        public TDto GetAsync<TDocument, TDto>(string url, string json = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                json = GetAsyncString(url).GetAwaiter().GetResult();
            }

            var notFoundModel = JsonConvert.DeserializeObject<NotFoundModel>(json);
            if (notFoundModel != null && notFoundModel.code != null)
            {
                var dto = (TDto)Activator.CreateInstance(typeof(TDto));
                dto.GetType().GetProperty("NotFound")?.SetValue(dto, notFoundModel);
                return dto;
            }
            else
            {
                if (default(TDto) != null) return (TDto)Convert.ChangeType(json, typeof(TDto));

                var jsonObject = JsonApiFramework.Json.JsonObject.Parse<JsonApiFramework.JsonApi.Document>(json);
                var context = (TDocument)Activator.CreateInstance(typeof(TDocument), jsonObject);

                if (jsonObject.IsDataCollectionDocument())
                {
                    return GetCollectionDto<TDto>(context);
                }

                return (TDto)context.GetType().GetMethod("GetResource", new[] { typeof(Type) }).Invoke(context, new object[] { typeof(TDto) });
            }
            
        }

        private TDto GetCollectionDto<TDto>(object context)
        {
            Type type = typeof(TDto).GetGenericArguments()[0];
            var contextModified = context.GetType().GetMethod("GetResourceCollection").Invoke(context, new object[] { type });
            var list = contextModified.GetType().GetMethod("ToList").Invoke(contextModified, null) as List<object>;
            var newList = (TDto)Activator.CreateInstance(typeof(TDto));

            foreach (var item in list as List<object>)
            {
                newList.GetType().GetMethod("Add").Invoke(newList, new object[] { item });
            }

            return newList;
        }
        #endregion
    }

    public class ValidationException : Exception
    {
        public List<ValidationFailure> Errors { get; set; }
        public ValidationException(string message) : base(message)
        {

        }
        public ValidationException() { }
    }

    public class InternalErrorException : Exception
    {

    }
    public class NotFoundException : Exception
    {
        public List<ValidationFailure> Errors { get; set; }
        public NotFoundException(string message) : base(message)
        {

        }
        public NotFoundException() { }
    }

}
