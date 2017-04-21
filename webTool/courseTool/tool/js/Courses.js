var fs = require("fs");
//当前exe所处路径
var rootPath = require("path").dirname(process.execPath);
//课程目录文件夹名
var coursesRootDirName = "courses";
//课程根目录
var coursesRootPath = rootPath + "/" + coursesRootDirName;
//配置文件后缀名
var configEx = ".txt";
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
  				if(!states.isDirectory()) {
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
			alert("非法路径：" + path);
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
			if (states.isDirectory()){
				var obj = new Object();
				obj.name = file;
				obj.path = path + "/" + file;
				folderList.push(obj);
			}
		}
	} else {
		alert("非法路径：" + path);
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
			return fs.readFileSync(filePath, "utf-8");    
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
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
}
 
//遍历读取文件
function readFiles(path,filesList) {
	if (fs.existsSync(path)){//是文件夹
		var files = fs.readdirSync(path);//需要用到同步读取
 		files.forEach(walk);
 		function walk(file) { 
  			states = fs.statSync(path+'/'+file);   
  			if(states.isDirectory()) {
   				readFiles(path+'/'+file,filesList);
  			} else { 
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

Courses.prototype.read = function() {
	this.images = getFiles(this.imagesPath);
	this.locale = getFiles(this.localePath);
	this.video = getFiles(this.videoPath);
	for (var i = 0, imax = this.images.length; i < imax; ++i) {
		console.log(this.images[i].name);
	}
	for (var i = 0, imax = this.video.length; i < imax; ++i) {
		console.log(this.video[i].name);
	}
	for (var i = 0, imax = this.locale.length; i < imax; ++i) {
		console.log(this.locale[i].name);
	}
}

Courses.prototype.readConfig = function(language){
	var name = language + configEx;
	for (var i in this.locale) {
		if (this.locale[i].name == name) {
			var config = new ConfigFile(this.locale[i].name, this.locale[i].path);
			config.read();
			this.configMap.put(language, config);
			return true;
		}
	}
	return false;
}



/////////////////////////////////////////////////////////////////////////
//ConfigFile
var courseId = "courseId";
var courseName = "courseName";
var courseTitle = "courseTitle";
var startStory = "startStory";
var storyImg = "img";
var storyText = "text";
var imgDirectioin = "directioin";
var imgPosition = "position";
var endStory = "endStory";
var allStepPage = "allStepPage";

function ConfigFile(fileName, path) {
	this.fileName = fileName;
	this.path = path;
	this.text = {};
	this.loadFlag = false;
}

ConfigFile.prototype.read = function() {
	try{
		var text = readFileTextSync(this.path);
		console.log(this.path + " 读取完成 text = " + text);
		this.text = JSON.parse(text);
		console.log("courseId = " + this.text["courseId"] + " courseName = " + this.text["courseName"]);
		this.loadFlag = true;
	}catch(e){
		//TODO handle the exception
		alert(e.toString());
	}
}

ConfigFile.prototype.getValue = function (key) {
	if (this.loadFlag && this.text != null) {
		return this.text[key];
	}
	return null;
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
	return this.text[startStory];
}

