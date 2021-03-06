﻿using Ekin.Clarizen.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ekin.Rest;

namespace Ekin.Clarizen.Files
{
    public class download : ISupportBulk
    {
        public Result.download Data { get; set; }
        public bool IsCalledSuccessfully { get; set; }
        public string Error { get; set; }
        public request BulkRequest { get; set; }

        public download(string serverLocation, string sessionId, Request.download request, bool isBulk = false)
        {
            if (request == null || String.IsNullOrEmpty(request.documentId))
            {
                IsCalledSuccessfully = false;
                this.Error = "Document id must be provided";
                return;
            }

            // Set the URL
            string url = (isBulk ? String.Empty : serverLocation) + "/files/download?documentId=" +
                         (request.documentId.Substring(0, 1) != "/" ? "/" : "") + request.documentId +
                         (request.redirect ? "&" + request.redirect.ToQueryString() : String.Empty);

            if (isBulk)
            {
                this.BulkRequest = new request(url, requestMethod.Get);
                return;
            }

            // Set the header for the authenticated user
            System.Net.WebHeaderCollection headers = new System.Net.WebHeaderCollection();
            headers.Add(System.Net.HttpRequestHeader.Authorization, String.Format("Session {0}", sessionId));

            // Call the API
            Ekin.Rest.Client restClient = new Ekin.Rest.Client(url, headers);
            restClient.ErrorType = typeof(error);
            Ekin.Rest.Response response = restClient.Get();

            // Parse Data
            if (response.Status == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    this.Data = JsonConvert.DeserializeObject<Result.download>(response.Content);
                    this.IsCalledSuccessfully = true;
                }
                catch (Exception ex)
                {
                    this.IsCalledSuccessfully = false;
                    this.Error = ex.Message;
                }
            }
            else
            {
                this.IsCalledSuccessfully = false;
                this.Error = response.InternalError.GetFormattedErrorMessage();
            }
        }

    }
}