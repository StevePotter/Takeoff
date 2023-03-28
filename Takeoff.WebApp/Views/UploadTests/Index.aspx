<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/internal.master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">

<%--    <h2>Direct to ScaleUp.  Client: plupload Flash</h2>
        <div id="s3Flash">ok</div>
--%>    <h2>Server: ScaleUp.  Client: Flash</h2>
        <div id="scaleUpHtml5">Select Files</div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="CssExternal" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="JsExternal" runat="server">
    <%=Html.JsLib("upload.js")%>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="JsInline" runat="server">
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="JsDocReady" runat="server">
</asp:Content>
