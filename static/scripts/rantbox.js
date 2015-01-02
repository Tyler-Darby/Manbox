var mh_running = false;
var editor = undefined;

window.onload = function()
{	
	// Poop out a code editor
	editor = CodeMirror.fromTextArea(document.getElementById("input"), {
		mode: "rant",
		lineNumbers: true,
		theme: "neat"
	});
	
	// Make it a reasonable size
	editor.setSize(450, 200);
	
	// Make it resizable
	$(editor.getWrapperElement()).resizable({
		resize: function() {
			editor.setSize($(this).width(), $(this).height());
		}
	});
	
	// Keyboard shortcut that doesn't work
	Mousetrap.bind("ctrl+enter", runPat);
	
	// Make the NSFW setting save to a cookie
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

var rantKeywords = [
	"[rs](?:ep)?", "n", "num", "after", "alt", "any", "arg", "before", "branch", "b", "break", "capsinfer", "case", "caps",
	"chance", "char", "close", "clrt", "cmp", "define", "dist", "else", "even", "extern", "(?:not)?first", "g", "generation",
	"get", "group", "if[n]?def", "is", "(?:not)?last", "len", "merge", "m", "(?:not)?middle", "nth", "numfmt", "mark", "match",
	"odd", "osend", "out", "repcount", "rc", "repindex", "ri", "repnum", "rn", "send", "src", "step", "undef", "x", "xnew",
	"xnone", "xpin", "xreset", "xseed", "xunpin", "rhymemode"
	].join("|");

var rantModes = [
	"lower", "upper", "sentence", "word", "title", "none",
	"normal", "roman(?:[-_](?:upper|lower))?", "group(?:[-_](?:commas|dots))?", "verbal[-_]en",
	"locked", "c?deck", "ordered", "reverse",
	"public", "private", "internal",
	"different", "(?:not[-_])?equal", "less", "greater", "none", "one", "all", "any",
	"perfect", "weak", "syllabic", "semirhyme", "forced", "slant[-_]rhyme", "pararhyme", "alliteration"
].join("|");

CodeMirror.defineSimpleMode("rant", {
	start: [
		{regex: /((?:^|[^\\])\[)(\?)/, token: [null, "keyword"]},
		{regex: /\\((?:\d+,)?(?:[^u\s\r\n]|u[0-9a-f]{4}))/, token: "string"},
		{regex: new RegExp("((?:^|[^\\\\])\\[)([$]\\w+|" + rantKeywords + ")(?:[:\\]])", "i"), token: [null, "keyword"]},
		{regex: new RegExp("([:;]\\s*)(" + rantModes + ")\\s*(?=[;\\]])", "i"), token: [null, "atom strong"]},
		{regex: /((?:^|[^\\])\[)(%[:=!]?\w+)(\s*(?:\+\^?\%?|\=\@?|![@^$]?|\$\^?|\@[?^$]?))?/, token: [null, "keyword", "strong"]},
		{regex: /(^|[^\\])\{\%\s*\w+\s*[^\\]\}/, token: "em"},
		{regex: /#.*/, token: "comment"},
		{regex: /\/\/(.*?[^\\])?\/\/i?/, token: "string"},
		{regex: /(^|[^\\])("(?:(?:[^"]|"")*)?")/, token: [null, "string"]},
		{regex: /(^|[^\\])(\<(?:.|[\r\n])*?[^\\]\>)/g, token: [null, "atom"]},
		{regex: /(^|[^\\])(\[)(\$\??)(\[.*?\])/, token: [null, null, "keyword", "special"]},
		{regex: /(?<)\*\s*\d+\s*\*/
	],
	meta: {
		dontIndentStates: ["comment"],
		lineComment: "#"
	}
});