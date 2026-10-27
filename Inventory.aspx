<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Inventory.aspx.cs" Inherits="Practical_8.Inventory" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Inventory Management</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="Content/style.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">

    <div class="layout">
        <aside class="sidebar">
            <div class="brand">📦 StockPro</div>
            <p class="side-label">WORKSPACE</p>
            <a class="nav-link active" href="Inventory.aspx">▦ &nbsp; Inventory</a>
            <div class="side-footer">Inventory Management<br />
                <span>XML Storage System</span>
            </div>
        </aside>

        <main class="main">
            <header class="topbar">
                <div>
                    <p class="eyebrow">WORKSPACE / INVENTORY</p>
                    <h1>Inventory Overview</h1>
                    <p class="muted">Manage your products and stock in one place.</p>
                </div>
                <div class="avatar">SP</div>
            </header>

            <section class="stats">
                <div class="stat-card">
                    <div class="stat-icon purple">▦</div>
                    <p>Total Products</p>
                    <h2><asp:Label ID="lblTotalProducts" runat="server" Text="0" /></h2>
                    <span class="stat-note">Products in inventory</span>
                </div>
                <div class="stat-card">
                    <div class="stat-icon blue">▤</div>
                    <p>Total Units</p>
                    <h2><asp:Label ID="lblTotalUnits" runat="server" Text="0" /></h2>
                    <span class="stat-note">Units available</span>
                </div>
                <div class="stat-card">
                    <div class="stat-icon orange">!</div>
                    <p>Low Stock</p>
                    <h2><asp:Label ID="lblLowStock" runat="server" Text="0" /></h2>
                    <span class="stat-note">1–5 units remaining</span>
                </div>
                <div class="stat-card">
                    <div class="stat-icon red">×</div>
                    <p>Out of Stock</p>
                    <h2><asp:Label ID="lblOutStock" runat="server" Text="0" /></h2>
                    <span class="stat-note">Products unavailable</span>
                </div>
            </section>

            <section class="panel">
                <div class="panel-heading">
                    <div>
                        <h2>Product Management</h2>
                        <p class="muted">Add, edit, and manage your products.</p>
                    </div>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="message"
                    EnableViewState="false" />

                <div class="form-grid">
                    <div class="field">
                        <label>Product Name</label>
                        <asp:TextBox ID="txtProductName" runat="server"
                            CssClass="input" MaxLength="100"
                            placeholder="e.g. Wireless Mouse" />
                    </div>
                    <div class="field">
                        <label>Category</label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="input">
                            <asp:ListItem>Electronics</asp:ListItem>
                            <asp:ListItem>Accessories</asp:ListItem>
                            <asp:ListItem>Furniture</asp:ListItem>
                            <asp:ListItem>Bags</asp:ListItem>
                            <asp:ListItem>Other</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label>Price (₹)</label>
                        <asp:TextBox ID="txtPrice" runat="server" CssClass="input"
                            TextMode="Number" placeholder="0.00" />
                    </div>
                    <div class="field">
                        <label>Quantity</label>
                        <asp:TextBox ID="txtQuantity" runat="server" CssClass="input"
                            TextMode="Number" placeholder="0" />
                    </div>
                </div>

                <asp:HiddenField ID="hfProductID" runat="server" />

                <div class="form-actions">
                    <asp:Button ID="btnAdd" runat="server" Text="+ Add Product"
                        CssClass="btn btn-primary" OnClick="btnAdd_Click" />
                    <asp:Button ID="btnUpdate" runat="server" Text="Save Changes"
                        CssClass="btn btn-success" OnClick="btnUpdate_Click"
                        Visible="false" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear Form"
                        CssClass="btn btn-light" OnClick="btnClear_Click"
                        CausesValidation="false" />
                </div>
            </section>

            <section class="panel">
                <div class="table-heading">
                    <div>
                        <h2>All Products</h2>
                        <p class="muted">Search and manage your inventory.</p>
                    </div>
                    <div class="filters">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="input search"
                            placeholder="Search products..."
                            AutoPostBack="true" OnTextChanged="FilterChanged" />
                        <asp:DropDownList ID="ddlFilterCategory" runat="server"
                            CssClass="input filter"
                            AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                            <asp:ListItem Value="">All Categories</asp:ListItem>
                            <asp:ListItem>Electronics</asp:ListItem>
                            <asp:ListItem>Accessories</asp:ListItem>
                            <asp:ListItem>Furniture</asp:ListItem>
                            <asp:ListItem>Bags</asp:ListItem>
                            <asp:ListItem>Other</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="table-wrap">
                    <asp:GridView ID="gvProducts" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="product-table"
                        GridLines="None"
                        DataKeyNames="ProductID"
                        EmptyDataText="No products found."
                        OnSelectedIndexChanged="gvProducts_SelectedIndexChanged"
                        OnRowDeleting="gvProducts_RowDeleting">
                        <Columns>
                            <asp:BoundField DataField="ProductID" HeaderText="ID" />
                            <asp:BoundField DataField="ProductName" HeaderText="PRODUCT" />
                            <asp:BoundField DataField="Category" HeaderText="CATEGORY" />
                            <asp:BoundField DataField="Price" HeaderText="PRICE (₹)"
                                DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Quantity" HeaderText="QUANTITY" />
                            <asp:TemplateField HeaderText="STOCK STATUS">
                                <ItemTemplate>
                                    <span class='<%# GetStockCss(Convert.ToInt32(Eval("Quantity"))) %>'>
                                        <%# GetStockText(Convert.ToInt32(Eval("Quantity"))) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="ACTIONS">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEdit" runat="server"
                                        Text="Edit" CommandName="Select"
                                        CssClass="action-link" CausesValidation="false" />
                                    &nbsp;
                                    <asp:LinkButton ID="btnDelete" runat="server"
                                        Text="Delete" CommandName="Delete"
                                        CssClass="action-link delete-link"
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Delete this product?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="table-footer">
                    Data is stored locally in the Products.xml file.
                </div>
            </section>
        </main>
    </div>

</form>
</body>
</html>
