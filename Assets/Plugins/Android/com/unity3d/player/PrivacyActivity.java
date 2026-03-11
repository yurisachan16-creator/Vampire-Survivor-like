package com.unity3d.player;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.ActivityNotFoundException;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Bundle;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebView;
import android.webkit.WebViewClient;
 
public class PrivacyActivity extends Activity implements DialogInterface.OnClickListener {
    private boolean useLocalHtml = true;
    private String privacyUrl = "";
    private AlertDialog currentDialog;
 
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
 
        ActivityInfo actInfo = null;
        try {
            //获取AndroidManifest.xml配置的元数据
            actInfo = this.getPackageManager().getActivityInfo(getComponentName(), PackageManager.GET_META_DATA);
            if (actInfo.metaData != null) {
                useLocalHtml = actInfo.metaData.getBoolean("useLocalHtml", true);
                privacyUrl = actInfo.metaData.getString("privacyUrl", "");
            }
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
        }
 
        //如果已经同意过隐私协议则直接进入Unity Activity
        if (GetPrivacyAccept()){
            EnterUnityActivity();
            return;
        }
        ShowPrivacyDialog();//弹出隐私协议对话框
    }
 
    @Override
    public void onClick(DialogInterface dialogInterface, int i) {
        switch (i){
            case AlertDialog.BUTTON_POSITIVE://点击同意按钮
                SetPrivacyAccept(true);
                EnterUnityActivity();//启动Unity Activity
                break;
            case AlertDialog.BUTTON_NEGATIVE://点击拒绝按钮,直接退出App
                finish();
                break;
        }
    }
    private void ShowPrivacyDialog(){
        final WebView webView = new WebView(this);
        webView.getSettings().setJavaScriptEnabled(false);
        webView.getSettings().setDomStorageEnabled(true);
        webView.setWebViewClient(new WebViewClient(){
            @Override
            public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
                if (request == null || request.getUrl() == null) {
                    return false;
                }
                return HandleExternalLink(request.getUrl().toString());
            }

            @Override
            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                return HandleExternalLink(url);
            }

            @Override
            public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
                if (useLocalHtml) {
                    return;
                }
                if (request == null || request.isForMainFrame()) {
                    view.stopLoading();
                    ShowLoadFailureDialog();
                }
            }
        });

        if (useLocalHtml || privacyUrl == null || privacyUrl.trim().isEmpty()){
            webView.loadDataWithBaseURL("https://localhost/", BuildLocalHtml(), "text/html", "UTF-8", null);
        }else{
            webView.loadUrl(privacyUrl);
        }
 
        AlertDialog.Builder privacyDialog = new AlertDialog.Builder(this);
        privacyDialog.setCancelable(false);
        privacyDialog.setView(webView);
        privacyDialog.setTitle("夜幕幸存者 用户协议与隐私政策");
        privacyDialog.setNegativeButton("取消",this);
        privacyDialog.setPositiveButton("确认",this);
        currentDialog = privacyDialog.create();
        currentDialog.show();
    }
//启动Unity Activity
    private void EnterUnityActivity(){
        Intent unityAct = new Intent();
        unityAct.setClassName(this, "com.unity3d.player.UnityPlayerActivity");
        this.startActivity(unityAct);
        finish();
    }
//保存同意隐私协议状态
    private void SetPrivacyAccept(boolean accepted){
        SharedPreferences.Editor prefs = this.getSharedPreferences("PlayerPrefs", MODE_PRIVATE).edit();
        prefs.putBoolean("PrivacyAccepted", accepted);
        prefs.apply();
    }
    private boolean GetPrivacyAccept(){
        SharedPreferences prefs = this.getSharedPreferences("PlayerPrefs", MODE_PRIVATE);
        return prefs.getBoolean("PrivacyAccepted", false);
    }

    private String BuildLocalHtml() {
        String resolvedUrl = privacyUrl == null || privacyUrl.trim().isEmpty() ? "#" : privacyUrl;
        return "<html><body style=\"padding:24px;font-size:16px;line-height:1.7;color:#222;\">"
                + "<h2>欢迎使用《夜幕幸存者》</h2>"
                + "<p>首次启动前，请阅读并同意《用户协议》与《隐私政策》。</p>"
                + "<p>点击下方链接将通过系统浏览器打开隐私页面。</p>"
                + "<p><a href=\"" + resolvedUrl + "\">查看《用户协议》与《隐私政策》</a></p>"
                + "</body></html>";
    }

    private boolean HandleExternalLink(String url) {
        if (url == null) {
            return false;
        }
        String normalized = url.trim();
        if (!(normalized.startsWith("http://") || normalized.startsWith("https://"))) {
            return false;
        }

        if (!OpenUrlInBrowser(normalized)) {
            ShowBrowserUnavailableDialog(normalized);
        }
        return true;
    }

    private boolean OpenUrlInBrowser(String url) {
        if (url == null || url.trim().isEmpty()) {
            return false;
        }

        try {
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
            intent.addCategory(Intent.CATEGORY_BROWSABLE);
            startActivity(intent);
            return true;
        } catch (ActivityNotFoundException ex) {
            return false;
        } catch (Exception ex) {
            return false;
        }
    }

    private void ShowBrowserUnavailableDialog(final String url) {
        if (isFinishing()) {
            return;
        }

        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setCancelable(true);
        builder.setTitle("无法打开浏览器");
        builder.setMessage("未检测到可用浏览器，请安装浏览器后重试。");
        builder.setNegativeButton("知道了", null);
        builder.setPositiveButton("重试", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialogInterface, int which) {
                OpenUrlInBrowser(url);
            }
        });
        builder.create().show();
    }

    private void ShowLoadFailureDialog() {
        if (isFinishing()) {
            return;
        }

        if (currentDialog != null && currentDialog.isShowing()) {
            currentDialog.dismiss();
        }

        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setCancelable(false);
        builder.setTitle("隐私政策加载失败");
        builder.setMessage("无法打开《夜幕幸存者》隐私政策页面，请检查网络后重试。当前 Android 安装包仅声明网络权限（INTERNET），用于首启时加载线上隐私政策页面。");
        builder.setNegativeButton("退出", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialogInterface, int which) {
                finish();
            }
        });
        builder.setPositiveButton("重试", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialogInterface, int which) {
                ShowPrivacyDialog();
            }
        });
        currentDialog = builder.create();
        currentDialog.show();
    }
}
