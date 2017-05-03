
//获取选项卡标签
function getTabbableTitle(activeFlag, li_id, href_target, text) {
	var dom = null;
	if (activeFlag) {
		dom = "<li class='active' id='" + li_id + "'>\n";
	} else {
		dom = "<li id='" + li_id + "'>\n";
	}
	dom += "<a href='#" + href_target + "' data-toggle='tab'>" + text + "</a>\n</li>";
	return dom;
}

//获取单个故事内容
function getStoryContent(activeFlag, storyContent, index, prefix, imgs) {
	var dom = null;
	if (activeFlag) {
		dom = "<div class='tab-pane active' id='" + prefix + "-panel-" + index + "'>\n";
	} else {
		dom = "<div class='tab-pane' id='" + prefix + "-panel-" + index + "'>\n";
	}
	//图片
	dom += "<div class='form-group'>\n";
	dom += getLabel(prefix + "-img-" + index, "col-sm-2 control-label", "图片");
	dom += "<div class='col-sm-10'>";
	var selectItem = "none";
	for (var i = 0; i < imgs.length; ++i) {
		if (null != storyContent && storyContent["img"] != undefined && endWith(storyContent["img"], imgs[i].name)) {
			selectItem = imgs[i].name;
		}
	}
	dom += getFileSelect("form-control", prefix + "-img-" + index, imgs, selectItem, true);
	dom += "</div>\n</div>\n";
	//文字
	dom += "<div class='form-group'>\n";
	dom += getLabel(prefix + "-text-"+index, "col-sm-2 control-label", "文字");
	dom += "<div class='col-sm-10'>";
	var textValue = "";
	if (null != storyContent && storyContent["text"] != undefined) {
		textValue = storyContent["text"];
	}
	dom += getInputOfText("form-control", prefix + "-text-"+index, "请输入对话文字", textValue);
	dom += "</div>\n</div>\n";
	//方向
	dom += "<div class='form-group'>\n";
	dom += getLabel(prefix + "-dir-"+index, "col-sm-2 control-label", "出现方向");
	dom += "<div class='col-sm-10' id='" + prefix + "-dir-"+ index + "'>";
	var directioin = "";
	if (null != storyContent && storyContent["directioin"] != undefined) {
		directioin = storyContent["directioin"];
	}
	dom += getSelect("form-control", prefix + "-dir-select-" + index, ["top", "bottom", "left", "right"], directioin, false);
	/*dom += getCheckbox_inline(prefix + "-dir-top", "top", directioin=="top");
	dom += getCheckbox_inline(prefix + "-dir-bottom", "bottom", directioin=="bottom");
	dom += getCheckbox_inline(prefix + "-dir-left", "left", directioin=="left");
	dom += getCheckbox_inline(prefix + "-dir-right", "right", directioin=="right");*/
	
	dom += "</div>\n</div>\n";
	//位置
	dom += "<div class='form-group'>\n";
	dom += getLabel(prefix + "-pos-"+index, "col-sm-2 control-label", "位置");
	dom += "<div class='col-sm-10'>";
	var pos = 0;
	if (null != storyContent && storyContent["position"] != undefined) {
		pos = storyContent["position"];
	}
	dom += getInputOfNumber("form-control", prefix + "-pos-"+index, "请输入数字", "[0-9]+", pos);
	dom += "</div>\n</div>\n";
	
	dom += "</div>\n";
	return dom;
}


function getStepPageContent(activeFlag, stepContent, index, imgs) {
	var dom;
	if (activeFlag) {
		dom = "<div class='tab-pane active' id='stepPage-panel-" + index + "'>\n";
	} else {
		dom = "<div class='tab-pane' id='stepPage-panel-" + index + "'>\n";
	}
	//标题
	dom += "<div class='form-group'>\n";
	dom += getLabel("stepPage-title-"+index, "col-sm-2 control-label", "标题");
	dom += "<div class='col-sm-10'>";
	var titleValue = "";
	if (null != stepContent) {
		titleValue = FindStepDataValue(stepContent, "title");
		if (titleValue == undefined) {
			titleValue = "";
		}
	}
	dom += getInputOfText("form-control", "stepPage-title-"+index, "请输入标题", titleValue);
	dom += "</div>\n</div>\n";
	
	//图片
	dom += "<div class='form-group'>\n";
	dom += getLabel("stepPage-img-" + index, "col-sm-2 control-label", "图片");
	dom += "<div class='col-sm-10'>";
	var selectItem = "none";
	var findValue = undefined;
	if (null != stepContent) {
		findValue = FindStepDataValue(stepContent, "img");	
	}
	for (var i = 0; i < imgs.length; ++i) {
		if (findValue != undefined && endWith(findValue, imgs[i].name)) {
			selectItem = imgs[i].name;
		}
	}
	dom += getFileSelect("form-control", "stepPage-img-"+index, imgs, selectItem, true);
	dom += "</div>\n</div>\n";
	
	//文字
	dom += "<div class='form-group'>\n";
	dom += getLabel("stepPage-text-" + index, "col-sm-2 control-label", "文字");
	dom += "<div class='col-sm-10'>";
	var textValue = "";
	if (null != stepContent) {
		textValue = FindStepDataValue(stepContent, "text");
		if (textValue == undefined) {
			textValue = "";
		}
	}
	dom += getInputOfText("form-control", "stepPage-text-"+index, "请输入描述文字", textValue);
	dom += "<p class='help-block'>若需要配置多条文字，请以|分开，例子：文字|文字|文字</p>";
	dom += "</div>\n</div>\n";
	
	//按钮
	//文字
	dom += "<div class='form-group'>\n";
	dom += getLabel("stepPage-btn-"+index, "col-sm-2 control-label", "按钮");
	dom += "<div class='col-sm-10'>";
	var btnValue = "";
	if (null != stepContent) {
		btnValue = FindStepDataValue(stepContent, "btn");
		if (btnValue == undefined) {
			btnValue = "";
		}
	}
	dom += getInputOfText("form-control", "stepPage-btn-"+index, "请输入按钮上的文字", btnValue);
	dom += "<p class='help-block'>如果没有按钮不需要填写该字段，若需要配置多个按钮，请以|分开，例子：按钮|按钮|按钮</p>";
	dom += "</div>\n</div>\n";
	
	dom += "</div>\n";
	return dom;
}

//获取选择列表
function getSelect(className, select_id, options, select_item , haveNoneFlag) {
	var dom = "<select class='" + className + "' id='" + select_id +"'>\n";
	if (options != undefined && options != null && options.length > 0) {
		if (haveNoneFlag) {
				if (select_item == "none") {
					dom += "<option value='none' selected='selected'>none</option>\n";
				}
				else {
					dom += "<option value='none'>none</option>\n";
				}
		}	
		for (var i = 0; i < options.length; ++i) {
			if (select_item == options[i]) {
				dom += "<option selected='selected' value='" + options[i] + "'>" + options[i] + "</option>\n";
			} else {
				dom += "<option value='" + options[i] + "'>" + options[i] + "</option>\n";
			}
			
		}
	}
	dom += "</select>\n";
	return dom;
}
//获取文件选择列表
function getFileSelect(className, id, fileList, select_file_name, haveNoneFlag) {
	var dom = "<select class='" + className + "' id='" + id +"'>\n";
	if (fileList != undefined && fileList != null && fileList.length > 0) {
		if (haveNoneFlag) {
				if (select_file_name == "none") {
					dom += "<option value='none' selected='selected'>none</option>\n";
				}
				else {
					dom += "<option value='none'>none</option>\n";
				}
		}	
		for (var i = 0; i < fileList.length; ++i) {
			if (select_file_name == fileList[i].name) {
				dom += "<option selected='selected' value='" + fileList[i].path + "'>" + fileList[i].name + "</option>\n";
			} else {
				dom += "<option value='" + fileList[i].path + "'>" + fileList[i].name + "</option>\n";
			}
			
		}
	}
	dom += "</select>\n";
	return dom;
}
//获取label
function getLabel(forTarget, className, text) {
	var dom = "<label for='" + forTarget +"' class='" + className + "'>" + text + "</label>";
	return dom;
}
//获取input-text
function getInputOfText(className, id, placeholder, value) {
	var dom = "<input type='text' class='" + className +"' id='" + id + "' placeholder='" + placeholder + "' value='" + value + "'>";
	return dom;
}
//获取input-number
function getInputOfNumber(className, id, placeholder, pattern, value) {
	var dom = "<input type='number' class='" + className +"' id='" + id + "' placeholder='" + placeholder + "' value='" + value + "' pattern='" + pattern + "'>";
	return dom;
}
//获取checkbox-inline
function getCheckbox_inline(value,  checkedFlag) {
	var dom = "<label class='checkbox-inline'>\n<input type='radio' name='optionsRadiosinline' value='" + value + "'";
	if (checkedFlag) {
		dom += " checked='checked'";
	}
	dom += ">" + value + "</label>\n";
	return dom;
}

function getFileName(o) {
    var pos=o.lastIndexOf("/");
    return o.substring(pos+1);  
}

function endWith(str, endStr) {
      var d=str.length-endStr.length;
      return (d>=0&&str.lastIndexOf(endStr)==d)
}
//stepPage数据查找
function FindStepDataValue(stepData, key) {
	for (var i = 0; i < stepData.length; ++i) {
		if (stepData[i]["key"] == key) {
			return stepData[i]["value"][0];
		}
	}
	return undefined;
}
