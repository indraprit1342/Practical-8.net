using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Practical_8
{
    public partial class Inventory : Page
    {
        private string XmlPath
        {
            get
            {
                return Server.MapPath("~/App_Data/Products.xml");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindProducts();
            }
        }

        // XML file ko read karna
        private XDocument LoadXml()
        {
            if (!File.Exists(XmlPath))
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(XmlPath));

                new XDocument(new XElement("Products")).Save(XmlPath);
            }

            return XDocument.Load(XmlPath);
        }

        // XML data ko DataTable mein convert karna
        private DataTable GetProductsTable()
        {
            DataTable table = new DataTable();

            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("Quantity", typeof(int));

            XDocument doc = LoadXml();

            foreach (XElement item in doc.Root.Elements("Product"))
            {
                table.Rows.Add(
                    (int)item.Element("ProductID"),
                    (string)item.Element("ProductName") ?? "",
                    (string)item.Element("Category") ?? "Other",
                    (decimal)item.Element("Price"),
                    (int)item.Element("Quantity")
                );
            }

            return table;
        }

        // Products aur dashboard display karna
        private void BindProducts()
        {
            DataTable table = GetProductsTable();

            lblTotalProducts.Text = table.Rows.Count.ToString();

            int totalUnits = 0;
            int lowStock = 0;
            int outStock = 0;

            foreach (DataRow row in table.Rows)
            {
                int quantity = Convert.ToInt32(row["Quantity"]);

                totalUnits += quantity;

                if (quantity == 0)
                    outStock++;
                else if (quantity <= 5)
                    lowStock++;
            }

            lblTotalUnits.Text = totalUnits.ToString();
            lblLowStock.Text = lowStock.ToString();
            lblOutStock.Text = outStock.ToString();

            // Search filter
            string search = txtSearch.Text.Trim();
            string category = ddlFilterCategory.SelectedValue;

            var filtered = table.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(row =>
                    row.Field<string>("ProductName")
                        .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    row.Field<string>("Category")
                        .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                filtered = filtered.Where(row =>
                    row.Field<string>("Category") == category);
            }

            DataTable result = table.Clone();

            foreach (DataRow row in filtered)
            {
                result.ImportRow(row);
            }

            gvProducts.DataSource = result;
            gvProducts.DataBind();
        }

        // Form data validate karna
        private bool TryGetFormValues(
            out string productName,
            out string category,
            out decimal price,
            out int quantity)
        {
            productName = txtProductName.Text.Trim();
            category = ddlCategory.SelectedValue;
            price = 0;
            quantity = 0;

            if (string.IsNullOrWhiteSpace(productName))
            {
                ShowMessage("Please enter product name.", true);
                return false;
            }

            if (!decimal.TryParse(
                txtPrice.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price) || price < 0)
            {
                ShowMessage("Please enter a valid price (0 or more).", true);
                return false;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity)
                || quantity < 0)
            {
                ShowMessage("Quantity must be a whole number (0 or more).", true);
                return false;
            }

            return true;
        }

        // CREATE: naya product add karna
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            string name, category;
            decimal price;
            int quantity;

            if (!TryGetFormValues(
                out name, out category, out price, out quantity))
                return;

            XDocument doc = LoadXml();

            int nextId = 1;

            if (doc.Root.Elements("Product").Any())
            {
                nextId = doc.Root.Elements("Product")
                    .Max(x => (int)x.Element("ProductID")) + 1;
            }

            XElement newProduct = new XElement("Product",
                new XElement("ProductID", nextId),
                new XElement("ProductName", name),
                new XElement("Category", category),
                new XElement("Price", price),
                new XElement("Quantity", quantity)
            );

            doc.Root.Add(newProduct);
            doc.Save(XmlPath);

            ClearForm();
            BindProducts();
            ShowMessage("Product added successfully!", false);
        }

        // READ: table mein kisi product ko select karna
        protected void gvProducts_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            int productId = Convert.ToInt32(
                gvProducts.SelectedDataKey.Value);

            XDocument doc = LoadXml();

            XElement product = doc.Root.Elements("Product")
                .FirstOrDefault(x =>
                    (int)x.Element("ProductID") == productId);

            if (product == null)
            {
                ShowMessage("Product not found.", true);
                return;
            }

            hfProductID.Value = productId.ToString();
            txtProductName.Text = (string)product.Element("ProductName");
            txtPrice.Text = ((decimal)product.Element("Price"))
                .ToString(CultureInfo.InvariantCulture);
            txtQuantity.Text = ((int)product.Element("Quantity")).ToString();

            string category = (string)product.Element("Category");

            if (ddlCategory.Items.FindByValue(category) != null)
                ddlCategory.SelectedValue = category;
            else
                ddlCategory.SelectedValue = "Other";

            btnAdd.Visible = false;
            btnUpdate.Visible = true;

            ShowMessage("Product loaded. Edit details and click Save Changes.", false);
        }

        // UPDATE: selected product ki details change karna
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            int productId;

            if (!int.TryParse(hfProductID.Value, out productId))
            {
                ShowMessage("Please select a product to update.", true);
                return;
            }

            string name, category;
            decimal price;
            int quantity;

            if (!TryGetFormValues(
                out name, out category, out price, out quantity))
                return;

            XDocument doc = LoadXml();

            XElement product = doc.Root.Elements("Product")
                .FirstOrDefault(x =>
                    (int)x.Element("ProductID") == productId);

            if (product == null)
            {
                ShowMessage("Product not found.", true);
                return;
            }

            product.Element("ProductName").Value = name;
            product.Element("Category").Value = category;
            product.Element("Price").Value =
                price.ToString(CultureInfo.InvariantCulture);
            product.Element("Quantity").Value = quantity.ToString();

            doc.Save(XmlPath);

            ClearForm();
            BindProducts();
            ShowMessage("Product updated successfully!", false);
        }

        // DELETE: product remove karna
        protected void gvProducts_RowDeleting(
            object sender, GridViewDeleteEventArgs e)
        {
            int productId = Convert.ToInt32(
                gvProducts.DataKeys[e.RowIndex].Value);

            XDocument doc = LoadXml();

            XElement product = doc.Root.Elements("Product")
                .FirstOrDefault(x =>
                    (int)x.Element("ProductID") == productId);

            if (product != null)
            {
                product.Remove();
                doc.Save(XmlPath);
                ShowMessage("Product deleted successfully!", false);
            }
            else
            {
                ShowMessage("Product not found.", true);
            }

            ClearForm();
            BindProducts();
            e.Cancel = true;
        }

        // Search aur category filter
        protected void FilterChanged(object sender, EventArgs e)
        {
            BindProducts();
        }

        // Form clear karna
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            ShowMessage("", false);
        }

        private void ClearForm()
        {
            hfProductID.Value = "";
            txtProductName.Text = "";
            txtPrice.Text = "";
            txtQuantity.Text = "";

            if (ddlCategory.Items.Count > 0)
                ddlCategory.SelectedIndex = 0;

            btnAdd.Visible = true;
            btnUpdate.Visible = false;

            gvProducts.SelectedIndex = -1;
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isError
                ? System.Drawing.Color.Firebrick
                : System.Drawing.Color.SeaGreen;
        }

        // Stock status ke liye CSS class
        protected string GetStockCss(int quantity)
        {
            if (quantity == 0)
                return "stock-badge stock-out";

            if (quantity <= 5)
                return "stock-badge stock-low";

            return "stock-badge stock-good";
        }

        protected string GetStockText(int quantity)
        {
            if (quantity == 0)
                return "Out of Stock";

            if (quantity <= 5)
                return "Low Stock";

            return "In Stock";
        }
    }
}