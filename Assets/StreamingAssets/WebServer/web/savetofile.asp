<%OPTION EXPLICIT%>
<%@  codepage="936" %>
<!--#include FILE="upfile_class.asp"-->
<%
dim upfile,FSPath,formName,oFile,upfilecount,msg,fso,saveto,reg 
upfilecount=0

set upfile=new upfile_class ''�����ϴ�����
set fso =server.CreateObject("scripting.filesystemobject")
set reg = new RegExp
reg.Global=false
reg.Pattern="^[^:*?""<>|]+$"
upfile.NoAllowExt="asp;exe;htm;html;aspx;cs;vb;js;"	'�����ϴ����͵ĺ�����
upfile.GetData (102400000)   'ȡ���ϴ�����,��������ϴ�100M (ʵ������Ӧ����10mb)
Response.charset="gb2312"

if upfile.isErr then  '�������
    select case upfile.isErr
	case 1
	Response.Write "��û���ϴ�����ѽ???�ǲ��Ǹ����??"
	case 2
	Response.Write "���ϴ����ļ��������ǵ�����,���100M"
	end select
	else

	for each formName in upfile.file '�г������ϴ��˵��ļ�
	   set oFile=upfile.file(formname)
		if  reg.Test(oFile.filename) then
		'ȡ�õ�ǰ�ļ��ڷ������е���������·��
		'saveto = Server.mappath("../"&ofile.filename)
		saveto = Server.mappath(ofile.filename)
		fspath = GetFilePath(saveto)	
	   if not fso.FolderExists(fspath)then
		hMkDir fspath
	   end if

	   upfile.SaveToFile formname, saveto ''�����ļ� Ҳ����ʹ��AutoSave������,����һ��,���ǻ��Զ������µ��ļ���
	
		if upfile.iserr then 
			msg= msg&upfile.errmessage
		else
			upfilecount=upfilecount+1
		end if
	else
		msg = "ָ�����ļ��� "&ofile.filename&" ���зǷ��ַ���"
	End if
	 set oFile=nothing
	next
	if msg<>""then
	Response.Write msg 
	else	
	Response.Write "200" 
	end if
end if
set upfile=nothing  'ɾ���˶���
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

'����·��ֻҪ·���ڵ��ϵ��ļ��в����ھ���㴴������'
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