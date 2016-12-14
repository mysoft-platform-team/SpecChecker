<%@ Page Language="C#"  %>
<script runat="server">
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        Response.Redirect("/Report.phtml");
    }
</script>