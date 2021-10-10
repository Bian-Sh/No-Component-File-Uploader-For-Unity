<%OPTION EXPLICIT%>
<%@  codepage="936" %>
<!--#include FILE="upfile_class.asp"-->
<%
dim upfile,FSPath,formName,oFile,upfilecount,msg,fso,saveto,reg 
upfilecount=0

set upfile=new upfile_class ''建立上传对象
set fso =server.CreateObject("scripting.filesystemobject")
set reg = new RegExp
reg.Global=false
reg.Pattern="^[^:*?""<>|]+$"
upfile.NoAllowExt="asp;exe;htm;html;aspx;cs;vb;js;"	'设置上传类型的黑名单
upfile.GetData (102400000)   '取得上传数据,限制最大上传100M (实际限制应该是10mb)
Response.charset="gb2312"

if upfile.isErr then  '如果出错
    select case upfile.isErr
	case 1
	Response.Write "你没有上传数据呀???是不是搞错了??"
	case 2
	Response.Write "你上传的文件超出我们的限制,最大100M"
	end select
	else

	for each formName in upfile.file '列出所有上传了的文件
	   set oFile=upfile.file(formname)
		if  reg.Test(oFile.filename) then
		'取得当前文件在服务器中的完整物理路径
		'saveto = Server.mappath("../"&ofile.filename)
		saveto = Server.mappath(ofile.filename)
		fspath = GetFilePath(saveto)	
	   if not fso.FolderExists(fspath)then
		hMkDir fspath
	   end if

	   upfile.SaveToFile formname, saveto ''保存文件 也可以使用AutoSave来保存,参数一样,但是会自动建立新的文件名
	
		if upfile.iserr then 
			msg= msg&upfile.errmessage
		else
			upfilecount=upfilecount+1
		end if
	else
		msg = "指定的文件名 "&ofile.filename&" 含有非法字符！"
	End if
	 set oFile=nothing
	next
	if msg<>""then
	Response.Write msg 
	else	
	Response.Write "200" 
	end if
end if
set upfile=nothing  '删除此对象
Response.End
%>

<%
function GetFilePath(FullPath)
  If FullPath <> "" Then
    GetFilePath = left(FullPath,InStrRev(FullPath, "\"))
    Else
    GetFilePath = ""
  End If
End function

'给个路径只要路径节点上的文件夹不存在就逐层创建出来'
Function hMkDir(fPath)
    Dim sp, k, strP
    If fPath = "" Then Exit Function
    if Right(fPath,1)="\" then
	strP = Mid(fPath,1,Len(fPath)-1)
    else
	strP= fPath
    end if	
    sp = Split(strP, "\")
    strP = ""
    Do While k < UBound(sp) + 1
	if strP="" then
	 strP = sp(0)
	else
	 strP=strP&"\"&sp(k)
	end if
        if not fso.FolderExists(strP) Then fso.CreateFolder strP
        k = k + 1
    Loop
End Function

%>