
$(function () {
	$("#nvaMenu a").click(function () {
		var link = $(this).attr("href");

		$("#reportContainer div.innerContainer").html("正在加载中，请稍后......").show();

		$.ajax({
			url: link,
			//headers: {"X-Gzip-Respond" : "1"},
			success: function (responseText, status, jqXhr) {
				$("#reportContainer div.innerContainer").html(responseText);
			}
		});
		return false;
	});

	$("a.sort").live("click", function () {
		var link = $(this).attr("href");

		$.ajax({
			url: link,
			//headers: { "X-Gzip-Respond": "1" },
			success: function (responseText, status, jqXhr) {
				$("#reportContainer div.innerContainer").html(responseText);
			}
		});
		return false;
	});


	$("p.next-page a").live("click", function () {
		var jthis = $(this);
		var height = jthis.parent().height();		

		$.ajax({
			url: jthis.attr("href"),
			//headers: { "X-Gzip-Respond": "1" },
			success: function (responseText, status, jqXhr) {
				// 追加新内容
				$("#reportContainer div.innerContainer").append(responseText);

				// 往下滚动一点，后面会隐藏【下一页】按键
				$('html, body').animate({
					scrollTop: ($(document).scrollTop() + height)
				});
				jthis.parent().hide();
			}
		});
		return false;
	});
});

