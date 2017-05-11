var fs = require("fs");
var rePath = require("path");
//当前exe所处路径
var rootPath = rePath.dirname(rePath.dirname(process.execPath));
console.log("rootPath = " + rootPath);
//课程目录文件夹名
var coursesRootDirName = "courses";
//课程根目录
var coursesRootPath = rootPath + "/" + coursesRootDirName;
//配置文件后缀名
var configEx = ".txt";

function isCoursesExists() {
	if (fs.existsSync(coursesRootPath)) {
		return true;
	}
	return false;
}

function getDirName(path) {
	return rePath.dirname(path);
}
//获取目录下的所有文件，包括子文件夹里面的文件
function getAllFiles(path) {
	
 	var filesList = [];
 	readFiles(path,filesList);
 	return filesList;
}
//获取目录下的文件
function getFiles(path) {
	var fileList = [];
	try{
		if (fs.existsSync(path)) {
			var files = fs.readdirSync(path);//需要用到同步读取
 			files.forEach(walk);
 			function walk(file) { 
  				states = fs.statSync(path+'/'+file);   
  				if(!states.isDirectory() && file != ".DS_Store" && path.indexOf(".svn") < 0 && path.indexOf(".git") < 0) {
   					//创建一个对象保存信息
   					var obj = new Object();
   					obj.size = states.size;//文件大小，以字节为单位
   					obj.name = file;//文件名
   					obj.path = path+'/'+file; //文件绝对路径
   					obj.withoutExName = file.substring(0, file.lastIndexOf("."));
   					fileList.push(obj);
  				}
 			}
		} else {
			console.log("非法路径：" + path);
		}
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
	return fileList;
}
//获取该路劲下的文件夹目录
function getFolder(path) {
	var folderList = [];
	if (fs.existsSync(path)) {
		var files = fs.readdirSync(path);
		files.forEach(walk);
		function walk(file) {
			states = fs.statSync(path + "/" + file);
			if (states.isDirectory() && file != ".svn" && file != ".git"){
				var obj = new Object();
				obj.name = file;
				obj.path = path + "/" + file;
				folderList.push(obj);
			}
		}
	} else {
		console.log("非法路径：" + path);
	}
	return folderList;
}
//文件夹不存在则创建文件夹
function createDir(path) {
	try{
		if (!fs.existsSync(path)) {
			fs.mkdirSync(imagesPath);    
		}
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
}

//读取文件
function readFileText(filePath, callback) {
	fs.readFile(filePath, function(err, data) {
		if (err) {
			console.log("读取文本:" + filePath + " 出错,error=" + err);  
			return callback("");
		}
    	return callback(data.toString());
	});  
}

function readFileTextSync(filePath) {
	try{
		if (fs.existsSync(filePath)) {
			return fs.readFileSync(filePath, "utf-8").toString();    
		} else {
			console.log(filePath + " 文件不存在");
		}
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
	return "";
}
function readFileTextSync(filePath, encoding) {
	try{
		if (fs.existsSync(filePath)) {
			return fs.readFileSync(filePath, encoding).toString();    
		} else {
			console.log(filePath + " 文件不存在");
		}
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
	return "";
}

// 写入数据, 文件不存在会自动创建
function writeFile(filePath, text, callback) {
	fs.writeFile(filePath, text, function (err) {
  		if (err) {
  			alert("写入文件:" + filePath + " 出错,error=" + err);  
			return callback(false);
  		}
  		return callback(true);
	});
}

function writeFileSync(filePath, text) {
	try{
		fs.writeFileSync(filePath, text, "utf-8");
		return true;
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
	return false;
}
 
//遍历读取文件
function readFiles(path,filesList) {
	if (fs.existsSync(path)){//是文件夹
		var files = fs.readdirSync(path);//需要用到同步读取
 		files.forEach(walk);
 		function walk(file) { 
  			states = fs.statSync(path+'/'+file);   
  			if(states.isDirectory()) {
  				if (file != ".svn" && file != ".git") {
  					readFiles(path+'/'+file,filesList);
  				}
  			} else if (file != ".DS_Store" && path.indexOf(".svn") < 0 && path.indexOf(".git") < 0){ 
   				//创建一个对象保存信息
   				var obj = new Object();
   				obj.size = states.size;//文件大小，以字节为单位
   				obj.name = file;//文件名
   				obj.path = path+'/'+file; //文件绝对路径
   				obj.withoutExName = file.substring(0, file.lastIndexOf("."));
   				filesList.push(obj);
  			}  
 		}
	} else {
		alert("非法路径：" + path);
	}
}




////////////////////////////////////////////////////////////////////////////////////////
//定义HashMap
function MyHashMap(){
    this.map = {};
}
MyHashMap.prototype.put = function(key, value){
        this.map[key] = value;
}
MyHashMap.prototype.get = function(key){
        if(this.map.hasOwnProperty(key)){
            return this.map[key];
        }
        return null;
}
MyHashMap.prototype.remove = function(key){
        if(this.map.hasOwnProperty(key)){
            return delete this.map[key];
        }
        return false;
}
MyHashMap.prototype.removeAll = function(){
        this.map = {};
}
MyHashMap.prototype.keySet = function(){
        var _keys = [];
        for(var i in this.map){
            _keys.push(i);
        }
        return _keys;
}

/////////////////////////////////////////////////////////////////////////////////
//Courses
function Courses(dirName, path){
	this.dirName = dirName;
	this.path = path;
	this.images = [];
	this.video = [];
	this.locale = [];
	this.imagesPath = path + "/images";
	this.localePath = path + "/locale";
	this.videoPath = path + "/video";
	createDir(this.imagesPath);
	createDir(this.localePath);
	createDir(this.videoPath);
	this.configMap = new MyHashMap();
	this.loadFlag = false;
}

Courses.prototype.read = function(lgType) {
	this.images = getFiles(this.imagesPath);
	this.locale = getFiles(this.localePath);
	this.video = getFiles(this.videoPath);
	var lgImgs = getFiles(this.imagesPath + "/" + lgType);
	for (var i = 0; i < lgImgs.length; ++i) {
		lgImgs[i].name = lgType + "/" + lgImgs[i].name;
		this.images[this.images.length] = lgImgs[i];
	}
}

Courses.prototype.readConfig = function(language) {
	var name = language + configEx;
	for (var i in this.locale) {
		if (this.locale[i].name == name) {
			var config = new ConfigFile(this.locale[i].name, this.locale[i].path, language);
			config.read();
			this.configMap.put(language, config);
			return true;
		}
	}
	return false;
}

Courses.prototype.getConfig = function(language) {
	return this.configMap.get(language);
}


/////////////////////////////////////////////////////////////////////////
//ConfigFile
var courseId = "courseId";
var courseName = "courseName";
var courseTitle = "courseTitle";
var startStory = "startStory";
var img = "img";
var text = "text";
var directioin = "directioin";
var position = "position";
var endStory = "endStory";
var allStepPage = "allStepPage";
var videoSrc = "videoSrc";
var isShowTrash = "isShowTrash";
var toolConfigShow = "toolConfigShow";
var toolConfig = "toolConfig";
var initProgram = "initProgram";
var standardProgram = "standardProgram";
var title = "title";
var btn = "btn";
var relativePathPrefix = "../../../";

function ConfigFile(fileName, path, lgType) {
	this.lgType = lgType;
	this.fileName = fileName;
	this.path = path;
	this.text = {courseId:0,courseName:"",courseTitle:"",startStory:[[]],endStory:[[]],allStepPage:[[]],videoSrc:"",
	isShowTrash:false,toolConfig:"<xml type=aaaaa>",toolConfigShow:[1,1,1,1,1,1,1],initProgram:"",standardProgram:""};
	this.loadFlag = false;
	console.log("fileName = " + fileName + " path=" + path + " lgType=" + lgType);
}

ConfigFile.prototype.read = function() {
	try{
		var text = readFileTextSync(this.path);
		console.log(this.path + " 读取完成");
		this.text = JSON.parse(text);
		console.log("courseId = " + this.text["courseId"] + " courseName = " + this.text["courseName"]);
		this.loadFlag = true;
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
}

ConfigFile.prototype.getCourseId = function () {
	return this.text[courseId];
}

ConfigFile.prototype.getCourseName = function () {
	return this.text[courseName];
}

ConfigFile.prototype.getCourseTitle = function () {
	return this.text[courseTitle];
}

ConfigFile.prototype.getStartStory = function () {
	if (this.text[startStory] != undefined) {
		return this.text[startStory][0];
	}
	return undefined;
}

ConfigFile.prototype.getEndStory = function () {
	if (this.text[endStory] != undefined) {
		return this.text[endStory][0];
	}
	return undefined;
}
ConfigFile.prototype.getStepPage = function () {
	if (this.text[allStepPage] != undefined) {
		return this.text[allStepPage];
	}
	return undefined;
}
ConfigFile.prototype.getVideoSrc = function () {
	return this.text[videoSrc];
}
ConfigFile.prototype.getShowTrash = function () {
	if (this.text.hasOwnProperty(isShowTrash)){
		return this.text[isShowTrash];
	}
	return false;
}
ConfigFile.prototype.getToolConfigShow = function () {
	return this.text[toolConfigShow];
}
ConfigFile.prototype.getToolConfig = function () {
	return this.text[toolConfig];
}
ConfigFile.prototype.getInitProgram = function () {
	return this.text[initProgram];
}
ConfigFile.prototype.getStandardProgram = function () {
	return this.text[standardProgram];
}

//set
ConfigFile.prototype.setCourseId = function (id) {
	if (id == "") {
		this.text[courseId] = 0;
	} else {
		this.text[courseId] = parseInt(id);
	}
}

ConfigFile.prototype.setCourseName = function (name) {
	this.text[courseName] = name;
}

ConfigFile.prototype.setCourseTitle = function (title) {
	this.text[courseTitle] = title;
}

ConfigFile.prototype.setStartStoryImg = function (index, imgPath) {
	console.log("imgPath=" + imgPath);
	if (imgPath == "none") {
		this.text[startStory][0][index][img] = "";
	} else {
		var path = relativePathPrefix + imgPath.substring(imgPath.indexOf(coursesRootDirName));
		this.text[startStory][0][index][img] = path;
	}
}
ConfigFile.prototype.setStartStoryText = function (index, txt) {
	this.text[startStory][0][index][text] = txt;
}
ConfigFile.prototype.setStartStoryDir = function (index, dir) {
	this.text[startStory][0][index][directioin] = dir;
}
ConfigFile.prototype.setStartStoryPos = function (index, pos) {
	if (pos == "") {
		this.text[startStory][0][index][position] = 0;
	} else {
		this.text[startStory][0][index][position] = parseInt(pos);
	}
}
ConfigFile.prototype.addStartStory = function () {
	if (this.text[startStory] == undefined) {
		this.text[startStory] = [[]];
	}
	var length = this.text[startStory][0].length;
	this.text[startStory][0][length] = {img:"", text:"", directioin:"top", position:0};
}

ConfigFile.prototype.setEndStoryImg = function (index, imgPath) {
	if (imgPath == "none") {
		this.text[endStory][0][index][img] = "";
	} else {
		var path = relativePathPrefix + imgPath.substring(imgPath.indexOf(coursesRootDirName));
		this.text[endStory][0][index][img] = path;
	}
}
ConfigFile.prototype.setEndStoryText = function (index, txt) {
	this.text[endStory][0][index][text] = txt;
}
ConfigFile.prototype.setEndStoryDir = function (index, dir) {
	this.text[endStory][0][index][directioin] = dir;
}
ConfigFile.prototype.setEndStoryPos = function (index, pos) {
	if (pos == "") {
		this.text[endStory][0][index][position] = 0;
	} else {
		this.text[endStory][0][index][position] = parseInt(pos);
	}
}
ConfigFile.prototype.addEndStory = function () {
	if (this.text[endStory] == undefined) {
		this.text[endStory] = [[]];
	}
	var length = this.text[endStory][0].length;
	this.text[endStory][0][length] = {img:"", text:"", directioin:"top", position:0};
}

ConfigFile.prototype.setStepPageTitle = function (index, titleText) {
	var dataIndex = findStepPageIndexForKey(this.text[allStepPage][index], title);
	if (titleText == "") {
		this.delStepPageData(index, dataIndex);
	} else {
		if (-1 == dataIndex) {
			var length = this.text[allStepPage][index].length;
			this.text[allStepPage][index][length] = {"key":title,"value":[titleText]};
		} else {
			this.text[allStepPage][index][dataIndex]["value"][0] = titleText;
		}
	}
}
ConfigFile.prototype.setStepPageText = function (index, textValue) {
	var dataIndex = findStepPageIndexForKey(this.text[allStepPage][index], text);
	if (textValue == "") {
		this.delStepPageData(index, dataIndex);
	} else {
		var textArray = [];
		if (textValue.indexOf("|") >= 0) {
			textArray = textValue.split("|");
		} else {
			textArray[0] = textValue;
		}
		if (-1 == dataIndex) {
			var length = this.text[allStepPage][index].length;
			this.text[allStepPage][index][length] = {"key":text,"value":textArray};
		} else {
			this.text[allStepPage][index][dataIndex]["value"] = textArray;
		}
	}
}
ConfigFile.prototype.setStepPageImg = function (index, imgPath) {
	var dataIndex = findStepPageIndexForKey(this.text[allStepPage][index], img);
	if (imgPath == "none") {
		this.delStepPageData(index, dataIndex);
	} else {
		var path = relativePathPrefix + imgPath.substring(imgPath.indexOf(coursesRootDirName));
		if (-1 == dataIndex) {
			var length = this.text[allStepPage][index].length;
			this.text[allStepPage][index][length] = {"key":img,"value":[path]};
		} else {
			this.text[allStepPage][index][dataIndex]["value"][0] = path;
		}
	}
	
}
ConfigFile.prototype.setStepPageBtn = function (index, btnValue) {
	var dataIndex = findStepPageIndexForKey(this.text[allStepPage][index], btn);
	if (btnValue == "") {
		this.delStepPageData(index, dataIndex);
	} else {
		var btnArray = [];
		if (btnValue.indexOf("|") >= 0) {
			btnArray = btnValue.split("|");
		} else {
			btnArray[0] = btnValue;
		}
		if (-1 == dataIndex) {
			var length = this.text[allStepPage][index].length;
			this.text[allStepPage][index][length] = {"key":btn,"value":btnArray};
		} else {
			this.text[allStepPage][index][dataIndex]["value"] = btnArray;
		}
	}
}
ConfigFile.prototype.delStepPageData = function (index, dataIndex) {
	if (-1 != dataIndex) {
		this.text[allStepPage][index].splice(dataIndex, 1);
	}
}
ConfigFile.prototype.addStepPage = function () {
	if (this.text[allStepPage] == undefined) {
		this.text[allStepPage] = [[]];
	}
	var length = this.text[allStepPage].length;
	this.text[allStepPage][length] = [];
}
ConfigFile.prototype.setVideoSrc = function (videoPath) {
	if (videoPath == "none") {
		this.text[videoSrc] = "";
	} else {
		var path = relativePathPrefix + videoPath.substring(videoPath.indexOf(coursesRootDirName));
		this.text[videoSrc] = path;
	}
	
}
ConfigFile.prototype.setShowTrash = function (showFlag) {
	this.text[isShowTrash] = showFlag;
}
ConfigFile.prototype.setToolConfigShow = function (index, showFlag) {
	if (showFlag) {
		this.text[toolConfigShow][index] = 1;
	} else {
		this.text[toolConfigShow][index] = 0;
	}
}
ConfigFile.prototype.setToolConfig = function (text) {
	this.text[toolConfig] = text;
}
ConfigFile.prototype.setInitProgram = function (text) {
	this.text[initProgram] = text;
}
ConfigFile.prototype.setStandardProgram = function (text) {
	this.text[standardProgram] = text;
}
ConfigFile.prototype.save = function (promptFlag) {
	var changeFlag = false;
	for (var i = 0; i < this.text[allStepPage].length; ++i) {
		if (this.text[allStepPage][i].length < 1) {
			this.text[allStepPage].splice(i, 1);
			i -= 1;
			changeFlag = true;
		}
	}
	if (writeFileSync(this.path, JSON.stringify(this.text))) {
		if (promptFlag) {
			alert("文件保存成功");
		}
	}
	return changeFlag;
}
ConfigFile.prototype.saveOther = function (lgArray, lgText) {
	var errerInfo = "";
	for (var i = 0; i < lgArray.length; ++i) {
		var otherConfig = new ConfigFile(this.fileName.replace(this.lgType, lgArray[i]), this.path.replace(this.lgType, lgArray[i]), lgArray[i]);
		otherConfig.text[courseId] = this.text[courseId];
		otherConfig.text[courseName] = getText(this.text[courseName], lgArray[i], lgText, errerInfo);
		otherConfig.text[courseTitle] = getText(this.text[courseTitle], lgArray[i], lgText, errerInfo);
		for (var storyIndex = 0; storyIndex < this.text[startStory][0].length; ++storyIndex) {
			if (otherConfig.text[startStory][0][storyIndex] == undefined) {
				otherConfig.text[startStory][0][storyIndex] = {};
			}
			if (this.text[startStory][0][storyIndex][img].indexOf("/" + this.lgType + "/")) {
				otherConfig.text[startStory][0][storyIndex][img] = this.text[startStory][0][storyIndex][img].replace("/" + this.lgType + "/", "/" + lgArray[i] + "/");
			} else {
				otherConfig.text[startStory][0][storyIndex][img] = this.text[startStory][0][storyIndex][img];
			}
			otherConfig.text[startStory][0][storyIndex][position] = this.text[startStory][0][storyIndex][position];
			otherConfig.text[startStory][0][storyIndex][directioin] = this.text[startStory][0][storyIndex][directioin];
			otherConfig.text[startStory][0][storyIndex][text] = getText(this.text[startStory][0][storyIndex][text], lgArray[i], lgText, errerInfo);
		}
		for (var storyIndex = 0; storyIndex < this.text[endStory][0].length; ++storyIndex) {
			if (otherConfig.text[endStory][0][storyIndex] == undefined) {
				otherConfig.text[endStory][0][storyIndex] = {};
			}
			if (this.text[endStory][0][storyIndex][img].indexOf("/" + this.lgType + "/")) {
				otherConfig.text[endStory][0][storyIndex][img] = this.text[endStory][0][storyIndex][img].replace("/" + this.lgType + "/", "/" + lgArray[i] + "/");
			} else {
				otherConfig.text[endStory][0][storyIndex][img] = this.text[endStory][0][storyIndex][img];
			}
			otherConfig.text[endStory][0][storyIndex][position] = this.text[endStory][0][storyIndex][position];
			otherConfig.text[endStory][0][storyIndex][directioin] = this.text[endStory][0][storyIndex][directioin];
			otherConfig.text[endStory][0][storyIndex][text] = getText(this.text[endStory][0][storyIndex][text], lgArray[i], lgText, errerInfo);
		}
		for (var stepIndex = 0; stepIndex < this.text[allStepPage].length; ++ stepIndex) {
			for (var dataIndex = 0; dataIndex < this.text[allStepPage][stepIndex].length; ++dataIndex) {
				if (otherConfig.text[allStepPage][stepIndex] == undefined) {
					otherConfig.text[allStepPage][stepIndex] = [];
				}
				if (otherConfig.text[allStepPage][stepIndex][dataIndex] == undefined) {
					otherConfig.text[allStepPage][stepIndex][dataIndex] = {};
				}
				otherConfig.text[allStepPage][stepIndex][dataIndex]["key"] = this.text[allStepPage][stepIndex][dataIndex]["key"];
				if (otherConfig.text[allStepPage][stepIndex][dataIndex]["value"] == undefined) {
					otherConfig.text[allStepPage][stepIndex][dataIndex]["value"] = [];
				}
				if (this.text[allStepPage][stepIndex][dataIndex]["key"] == img) {
					if (this.text[allStepPage][stepIndex][dataIndex]["value"][0].indexOf("/" + this.lgType + "/")) {
						otherConfig.text[allStepPage][stepIndex][dataIndex]["value"][0] = this.text[allStepPage][stepIndex][dataIndex]["value"][0].replace("/" + this.lgType + "/", "/" + lgArray[i] + "/");
					} else {
						otherConfig.text[allStepPage][stepIndex][dataIndex]["value"][0] = this.text[allStepPage][stepIndex][dataIndex]["value"][0];
					}
					
				} else {
					for (var valueIndex = 0; valueIndex < this.text[allStepPage][stepIndex][dataIndex]["value"].length; ++valueIndex) {
						otherConfig.text[allStepPage][stepIndex][dataIndex]["value"][valueIndex] = getText(this.text[allStepPage][stepIndex][dataIndex]["value"][valueIndex], lgArray[i], lgText, errerInfo);
					}
				}
			}
		}
		otherConfig.text[videoSrc] = this.text[videoSrc];
		otherConfig.text[isShowTrash] = this.text[isShowTrash];
		otherConfig.text[toolConfig] = this.text[toolConfig];
		otherConfig.text[toolConfigShow] = this.text[toolConfigShow];
		otherConfig.text[initProgram] = this.text[initProgram];
		otherConfig.text[standardProgram] = this.text[standardProgram];
		otherConfig.save(false);
	}
	if (errerInfo != "") {
		alert(errerInfo);
	}
}
function getText(key, lgType, lgText, errerInfo) {
	if (lgText.hasOwnProperty(key) && lgText[key].hasOwnProperty(lgType)) {
		return lgText[key][lgType];
	}
	errerInfo += key + " " + lgType + " 翻译不存在\n";
	return key;
}

function findStepPageIndexForKey(stepData, key) {
	for (var i = 0; i < stepData.length; ++i) {
		if (stepData[i]["key"] == key) {
			return i;
		}
	}
	return -1;
}
