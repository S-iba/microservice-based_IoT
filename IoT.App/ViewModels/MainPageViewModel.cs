using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace IoT.App.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private HubConnection _hubConnection;
        private const string ApiBaseUrl = "http://10.0.2.2:5257"; //TODO: Update with your actual API URL

        
        private string _statusText = "System Idle";
        private bool _isTriggered = true;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }
        public bool IsTriggered
        {
            get => _isTriggered;
            set => SetProperty(ref _isTriggered, value);
        }

        public MainPageViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            InitializeSignalRAsync();
        }

        private async void InitializeSignalRAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{ApiBaseUrl}/notifications")
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.On<string>("ReceiveStatusUpdate", (message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusText = message;
                });
            });

            try
            {
                await _hubConnection.StartAsync();
                StatusText = "Connected to Real-time Hub";
            }
            catch (Exception ex)
            {
                StatusText = $"Failed to connect to Real-time Hub: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task OnTriggerClickedAsync(string actionType)
        {
            IsTriggered = false;
            StatusText = $"Sending {actionType}...";

            try
            {
                var command = new { ActionType = actionType };
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/Command", command);

                if (response.IsSuccessStatusCode)
                {
                    StatusText = "Command accepted by Queue";

                }else
                {
                    StatusText = $"Failed to send command: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Connection Failed: {ex.Message}";
            }
            finally
            {
                IsTriggered = true;

            }

    }
}
}
