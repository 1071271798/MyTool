function CookieTool(){
	this.setCookies = setCookies;
	function setCookies(ckname, ckvalue){
		var time = new Date();
		time.setTime(time.getTime() + 10000*24*60*60*1000);
		var con = escape(ckname) + "=" + escape(ckvalue) + ";expires=" + time.toGMTString();
		console.log(con);
		document.cookie = con;
	}
	this.getCookies = getCookies;
	function getCookies(ckname){
		var exp = escape(ckname) + "=[^;]*";
		var reg = new RegExp(exp);
		var arg = reg.exec(document.cookie);
		if (null != arg){
			var tmp = arg[0].split('=');
			if (tmp.length >= 2){
				console.log(unescape(tmp[1]));
				return unescape(tmp[1]);
			}
			return null;
		}
		return null;
	}
	this.delCookies = delCookies;
	function delCookies(ckname){
		var time = new Date();
		time.setTime(time.getTime() - 1);
		var cval=getCookies(ckname);
		if(cval!=null)
			document.cookie= escape(ckname) + "="+cval+";expires="+time.toGMTString();
	}
	this.setCookiesTotal = setCookiesTotal;
	function setCookiesTotal(ckname, ckvalue, day, path, domain){
		var time = new Date();
		time.setTime(time.getTime() + day*24*60*60*1000);
		document.cookie = escape(ckname) + "=" + escape(ckvalue) + ";expires=" + time.toGMTString() + ";path=" + path + ";domain=" + domain;
	}
}
