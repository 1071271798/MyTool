var languageType = ["zh-hans", "en", "zh-hant", "fr", "de", "it", "es", "ja", "pt", "ar", "ko", "ru", "pl"];
var languageType_CN = ["简体中文", "英文", "繁体中文", "法语", "德语", "意大利语", "西班牙语", "日语", "葡萄牙", "阿拉伯语", "韩语", "俄语", "波兰"];
var currentLanguage = "zh-hans";


function LanguageTool(lgKey, fileText) {
	this.lgKey = lgKey;
	console.log("select language = " + lgKey);
	this.lgText = {};
	this.loadFlag = false;
	this.initText(fileText);
}
	
LanguageTool.prototype.initText = function(fileText) {
	var textArray = fileText.split("\r");
	if (textArray.length > 0) {
		console.log(textArray[0]);
		var lgTypeArray = null;
		if (textArray[0].indexOf("zh-hans") >= 0) {
			lgTypeArray = languageType;
		} else if (textArray[0].indexOf("简体中文") >= 0) {
			lgTypeArray = languageType_CN;
		} else {
			alert("未读取到语言标志，请选择正确的翻译文件");
			return;
		}
		if (textArray[0].indexOf("\t") < 0) {
			alert("未检测到分隔符，请导出文件的时候选择制表符分隔");
		} else if (null != lgTypeArray){
			var readLgType = trim(textArray[0], "\n").split("\t");
			if (null != readLgType) {
				console.log(readLgType);
			}
			var lgTypeArrayNew = [];
			for (var i = 0; i < readLgType.length; ++i) {
				var findFlag = false;
				for (var index = 0; index < lgTypeArray.length; ++index) {
					if (readLgType[i]!="" && readLgType[i]!= null && (readLgType[i] == lgTypeArray[index] || readLgType[i].indexOf(lgTypeArray[index]) >= 0)){
						
						lgTypeArrayNew[i] = languageType[index];
						findFlag = true;
					}
				}
				if (!findFlag) {
					lgTypeArrayNew[i] = readLgType[i];
				}
			}
			var keyIndex = lgTypeArrayNew.indexOf(this.lgKey);
			if (-1 == keyIndex || keyIndex == undefined) {
				alert("未在翻译文件中找到所选语言，请选择正确的翻译文件");
				return;
			}
			for (var i = 1; i < textArray.length; ++i) {
				var tmpText = trim(textArray[i], "\n");
				if (tmpText == "") {
					continue;
				}
				var tmplg = tmpText.split("\t");
				if (tmplg.length != readLgType.length) {
					alert("翻译文件第" + (i + 1) + "行数据有问题，请检查内容中是否包含制表符");
					return;
				}
				for (var lgIndex = 0; lgIndex < tmplg.length; ++lgIndex) {
					if (lgIndex != keyIndex && tmplg[lgIndex] != null && tmplg[lgIndex] != "") {
						if (this.lgText[tmplg[keyIndex]] == undefined) {
							this.lgText[tmplg[keyIndex]] = {};
						}
						this.lgText[tmplg[keyIndex]][lgTypeArrayNew[lgIndex]] = trim(tmplg[lgIndex], "\"");
					}
				}
			}
			this.loadFlag = true;
		}
	} else {
		alert("未读取到翻译内容，请选择正确的翻译文件");
	}
}

LanguageTool.prototype.getText = function(key, lgType) {
	if (this.loadFlag && this.lgText.hasOwnProperty(key) && this.lgText[key].hasOwnProperty(lgType)) {
		return this.lgText[key][lgType];
	}
	return undefined;
}

LanguageTool.prototype.getLanguageJson = function() {
	if (this.loadFlag) {
		var language = {};
		for (var textKey in this.lgText) {
			for (var lgKey in this.lgText[textKey]) {
				if (!language.hasOwnProperty(lgKey)) {
					language[lgKey] = {};
				}
				language[lgKey][textKey] = this.lgText[textKey][lgKey];
			}
		}
		return language;
	}
	return undefined;
}

function trimStart(str, trimStr){
    if(!trimStr){return str;}
    var temp = str;
    while(true){
        if(temp.substring(0,trimStr.length)!=trimStr){
            break;
        }
        temp = temp.substring(trimStr.length);
    }
    return temp;
}
function trimEnd(str, trimStr){
    if(!trimStr){return str;}
    var temp = str;
    while(true){
        if(temp.substring(temp.length-trimStr.length,trimStr.length)!=trimStr){
            break;
        }
        temp = temp.substring(0,temp.length-trimStr.length);
    }
    return temp;
}
function trim(str, trimStr){
    var temp = trimStr;
    if(!trimStr){temp=" ";}
    return trimEnd(trimStart(str, temp) ,temp);
}
