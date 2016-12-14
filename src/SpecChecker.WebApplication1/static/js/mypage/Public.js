
var MessageWnd = {
	hide: function () {
		$("#divMessage div.okPanel").hide();
		$("#divMessage div.yesnoPanel").hide();
		$("#divMessage").hide();
		$("#divShade").hide();
		$("#divMessage a.yes").unbind("click");
	},
	alert: function (message) {
		$("#divMessage div.messageText").html(message);
		$("#divMessage div.okPanel").show();
		$("#divShade").show();
		$("#divMessage").slideDown(300);
	},
	confirm: function (message, callback) {
		$("#divMessage div.messageText").html(message);
		$("#divMessage div.yesnoPanel").show();
		$("#divMessage a.yes").click(function () { MessageWnd.hide(); callback(); });
		$("#divShade").show();
		$("#divMessage").slideDown(300);
	}
};


var PublicFunc = {
	ValidateControl: function (expression, message) {
		if ($.trim($(expression).val()).length == 0) {
			MessageWnd.alert(message);
			return false;
		}
		return true;
	}
    ,
	AjaxBeforeSend: function (xhr) {
		/*
			if( typeof(g__oldIE) == "boolean" && g__oldIE ){
				alert("您的IE浏览器版本太低，建议升级IE版本，或者使用 FireFox, Opera, Safari, Chrome 之类的浏览器。");
				return false;
			}
		*/
		if (jQuery.active > 1)
			return false;

		if (this.url.indexOf("javascript:") == 0)
			return false;

		$("#ajaxMessage").show();

		if (this.currentSubmitButton)
			this.currentSubmitButton.attr("disabled", "disabled");
	},
	AjaxComplete: function (xhr) {
		$("#ajaxMessage").hide();
		if (this.currentSubmitButton)
			this.currentSubmitButton.removeAttr("disabled");

	},
	AjaxError: function (xhr, textStatus, errorThrown) {
		if (xhr.responseText) {
			var start = xhr.responseText.indexOf("<title>");
			var end = xhr.responseText.indexOf("</title>");
			if (start > 0 && end > start) {
				MessageWnd.alert(xhr.responseText.substring(start + 7, end));
				return;
			}
		}
		MessageWnd.alert("调用服务器失败。");
	}

};


function __setSidebarHeigth() {
	var height1 = $("#leftPanel").height();
	if (height1 > $("#rightPanel").height())
		$("#rightPanel").height(height1);
}

// 纠正Opera问题
document.onreadystatechange = function () {
	if (document.readyState == "complete")
		__setSidebarHeigth();
}


$(function () {
	// 设置Ajax操作的默认设置
	$.ajaxSetup({
		//beforeSend: PublicFunc.AjaxBeforeSend,
		//complete: PublicFunc.AjaxComplete,
		error: PublicFunc.AjaxError
	});

	// 设置侧边栏的高度
	__setSidebarHeigth();


	// 绑定消息提示层的关闭按钮事件。
	$("#divMessage a.close").click(MessageWnd.hide);

});


