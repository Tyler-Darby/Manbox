var mh_running = false;
var editor = undefined;

window.onload = function()
{	
	editor = CodeMirror.fromTextArea(document.getElementById("input"), {
		mode: "rant",
		lineNumbers: true
	});
	editor.setSize(450, 200);
	
	var cbNsfw = document.getElementById("nsfw");
	cbNsfw.checked = $.cookie("nsfw") === "true";
	cbNsfw.onchange = function() { $.cookie("nsfw", cbNsfw.checked); }

	if (window.location.hash.length > 0)
	{
		mh_running = true;
		$.ajax({
			type: "POST",
			url: "/rantbox/fetch",
			data: { Hash: window.location.hash }
		})
		
		.done(function(output)
		{
			mh_running = false;
			editor.setValue(output);
			runPat();
		});
	}
}

function createHashLink(hash)
{
	var endPoint = window.location.href.indexOf('#');
	if (endPoint === -1)
	{
		endPoint = window.location.href.indexOf('?');
	}
	if (endPoint === -1)
	{
		endPoint = window.location.href.length;
	}
	
	return window.location.href.slice(0, endPoint) + '#' + (hash[0] == '#' ? hash.substring(1) : hash);
}
	
function runPat()
{
	if (mh_running)
	{
		return;
	}
	
	mh_running = true;
	var loading = document.getElementById("loading");
	loading.style.visibility = "visible";
	loading.style.opacity = 100;
	
	var pattern = editor.getValue();
	var dirty = document.getElementById("nsfw").checked;
	
	$.ajax({
		type: "POST",
		url: "/rantbox/run",
		data: { code: pattern, nsfw: dirty }
	})
	
	.done(function(response)
	{
		var json = JSON.parse(response);
		
		var outputBox = $("#output");
		outputBox.val(json["result"]);
		
		if (json["success"] == "ok")
		{
			outputBox.css("color", "#000");
		}
		else if (json["success"] == "fail")
		{
			outputBox.css("color", "#c00");
		}
		
	    $("#hash").val(createHashLink(json["hash"]));
		$("#hashfield").css("display", "block");
		
		var loading = document.getElementById("loading");
		loading.style.visibility = "hidden";
		loading.style.opacity = 0;
		
		window.location.hash = "#" + json["hash"];		
		mh_running = false;
	});		
}

Mousetrap.bind("ctrl+enter", runPat);

var keywords = [
	"[rs](?:ep)?", "n", "num", "after", "all", "any", "arg", "before", "branch", "break", "capsinfer", "case", "caps",
	"chance", "char", "close", "clrt", "cmp", "define", "dist", "else", "even", "extern", "(?:not)?first", "g", "generation",
	"get", "group", "if[n]?def", "is", "(?:not)?last", "len", "merge", "(?:not)?middle", "nth", "numfmt", "mark", "match",
	"odd", "osend", "out", "repcount", "rc", "repindex", "ri", "repnum", "rn", "send", "src", "step", "undef", "x", "xnew",
	"xnone", "xpin", "xreset", "xseed", "xunpin"
	].join("|");

CodeMirror.defineSimpleMode("rant", {
	start: [
		{regex: /\\((?:\d+,)?(?:[^u\s\r\n]|u[0-9a-f]{4}))/, token: "string"},
		{regex: new RegExp("((?:^|[^\\\\])\\[)(\$\w+|" + keywords + ")(?:[:\\]]|$)", "i"), token: [null, "def"]},
		{regex: /#.*/, token: "comment"},
		{regex: /\/(.*?[^\\])?\/i?/, token: "string"},
		{regex: /(^|[^\\])("(?:(?:[^"]|"")*)?")/, token: [null, "string"]},
		{regex: /(^|[^\\])(\<(?:.|[\r\n])*?[^\\]\>)/g, token: [null, "string-2"]},
		{regex: /((?:^|[^\\])\[)(\$\??)(\[.*?\])/, token: [null, "qualifier", "def"]}
	],
	meta: {
		dontIndentStates: ["comment"],
		lineComment: "#"
	}
});