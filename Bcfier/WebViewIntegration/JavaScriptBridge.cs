﻿using CefSharp;
using Newtonsoft.Json;

namespace Bcfier.WebViewIntegration
{
  public class JavaScriptBridge
  {
    /// <summary>
    /// This is the name of the global window object that's set in JavaScript, e.g.
    /// 'window.RevitBridge'.
    /// </summary>
    public const string REVIT_BRIDGE_JAVASCRIPT_NAME = "RevitBridge";

    public const string REVIT_READY_EVENT_NAME = "revit.plugin.ready";

    private JavaScriptBridge()
    {
      // This class should only be available as a singleton, access
      // via the static 'Instance' property.
    }

    private IWebBrowser _webBrowser;
    public static JavaScriptBridge Instance { get; } = new JavaScriptBridge();

    public event WebUIMessageReceivedEventHandler OnWebUIMessageReveived;
    public delegate void WebUIMessageReceivedEventHandler(object sender, WebUIMessageEventArgs e);

    public void SetWebBrowser(IWebBrowser webBrowser)
    {
      if (_webBrowser != null)
      {
        throw new System.InvalidOperationException("The web browser instance was already set in the " + nameof(JavaScriptBridge));
      }

      _webBrowser = webBrowser;

      _webBrowser.LoadingStateChanged += (s, e) => isLoaded = true;

    }

    private bool isLoaded = false;

    public void SendMessageToRevit(string messageType, string trackingId, string messagePayload)
    {
      if (!isLoaded)
      {
        return;
      }
      var eventArgs = new WebUIMessageEventArgs(messageType, trackingId, messagePayload);
      OnWebUIMessageReveived?.Invoke(this, eventArgs);
    }

    public void SendMessageToOpenProject(string messageType, string trackingId, string messagePayload)
    {
      if (!isLoaded)
      {
        return;
      }
      var messageData = JsonConvert.SerializeObject(new { messageType, trackingId, messagePayload });
      var encodedMessage = JsonConvert.ToString(messageData);
      _webBrowser?.GetMainFrame()
        .ExecuteJavaScriptAsync($"{REVIT_BRIDGE_JAVASCRIPT_NAME}.sendMessageToOpenProject({encodedMessage})");
    }
  }
}