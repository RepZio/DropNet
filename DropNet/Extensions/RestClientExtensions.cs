﻿/* Code below modified from a version taken from Laurent Kempé's blog
 * http://www.laurentkempe.com/post/Extending-existing-NET-API-to-support-asynchronous-operations.aspx
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Threading;
using System.Net;
using DropNet.Exceptions;

namespace DropNet.Extensions
{
    public static class RestClientExtensions
    {
        public static Task<TResult> ExecuteTask<TResult>(this RestClient client,
                                                         RestRequest request) where TResult : new()
        {
            var tcs = new TaskCompletionSource<TResult>();

            WaitCallback
                asyncWork = _ =>
                {
                    try
                    {
#if WINDOWS_PHONE
                                    //check for network connection
                                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                                    {
                                        tcs.SetException(new DropboxException
                                        {
                                            StatusCode = System.Net.HttpStatusCode.BadGateway
                                        });
                                        return;
                                    }
#endif
                        client.ExecuteAsync<TResult>(request,
                                                     (response) =>
                                                     {
                                                         if (response.StatusCode != HttpStatusCode.OK)
                                                         {
                                                             tcs.SetException(new DropboxException(response));
                                                         }
                                                         else
                                                         {
                                                             tcs.SetResult(response.Data);
                                                         }
                                                     });
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                };

            return ExecuteTask(asyncWork, tcs);
        }


        public static Task<RestResponse> ExecuteTask(this RestClient client,
                                                         RestRequest request)
        {
            var tcs = new TaskCompletionSource<RestResponse>();

            WaitCallback
                asyncWork = _ =>
                {
                    try
                    {
#if WINDOWS_PHONE
                                    //check for network connection
                                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                                    {
                                        tcs.SetException(new DropboxException
                                        {
                                            StatusCode = System.Net.HttpStatusCode.BadGateway
                                        });
                                        return;
                                    }
#endif
                        client.ExecuteAsync(request,
                                                     (response) =>
                                                     {
                                                         if (response.StatusCode != HttpStatusCode.OK)
                                                         {
                                                             tcs.SetException(new DropboxException(response));
                                                         }
                                                         else
                                                         {
                                                             tcs.SetResult(response);
                                                         }
                                                     });
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                };

            return ExecuteTask(asyncWork, tcs);
        }

        private static Task<TResult> ExecuteTask<TResult>(WaitCallback asyncWork,
                                                          TaskCompletionSource<TResult> tcs)
        {
            ThreadPool.QueueUserWorkItem(asyncWork);

            return tcs.Task;
        }
    }
}