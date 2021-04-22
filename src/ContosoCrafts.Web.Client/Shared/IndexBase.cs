using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ContosoCrafts.Web.Client.Shared
{
    public class IndexBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        private IHttpClientFactory ClientFactory { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        private ILogger<IndexBase> logger { get; set; }

        [Inject]
        private IToastService toastService { get; set; }

        private HubConnection hubConnection;
        private IJSObjectReference module;

        protected override async Task OnInitializedAsync()
        {
            logger.LogInformation("OnInitializedAsync called");

            hubConnection = new HubConnectionBuilder()
                        .WithUrl(NavigationManager.ToAbsoluteUri("/events"))
                        .Build();

            hubConnection.On<string, CheckoutResponse>("CheckoutSessionStarted", async (pubKey, chkResp) =>
            {
                logger.LogInformation("CheckoutSessionStarted fired");

                if (module == null)
                {
                    module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/stripe.js");
                }

                await module.InvokeVoidAsync("checkout", pubKey, chkResp.CheckoutSessionID);

            });

            hubConnection.On<string>("CheckoutCompleted", (status) =>
            {
                if (status == "success")
                    toastService.ShowSuccess("Checkout completed", "Success");
                else if (status == "failure")
                    toastService.ShowError("Unable to process payment", "Payment Failed");
            });

            await hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();

            if (module != null)
            {
                await module.DisposeAsync();
            }
        }
    }
}