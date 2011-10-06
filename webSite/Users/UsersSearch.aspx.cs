﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Orion.Entity2;
public partial class Users_UsersSearch : System.Web.UI.Page
{
    #region Variable declare
    private String EntityTypeName
    {
        get
        {
            return "UserSearch";
        }
    }

    private Users _Entity
    {
        get
        {
            Object o;
            o = ViewState["Current_Entity_" + EntityTypeName];

            if (o == null)
            {
                o = new Users();

                ViewState["Current_Entity_" + EntityTypeName] = o;
            }
            return o as Users;

        }
        set
        {
            ViewState["Current_Entity_" + EntityTypeName] = value;
        }
    }
    private String Filter
    {
        get
        {
            object o = ViewState["Current_Filter"];
            if (o == null)
            {
                return "";
            }
            else
                return o.ToString();
        }
        set
        {
            ViewState["Current_Filter"] = value;
        }
    }
    private String OrderBy
    {
        get
        {
            object o = ViewState["Current_OrderBy"];
            if (o == null)
            {
                return "";
            }
            else
                return o.ToString();
        }
        set
        {
            ViewState["Current_OrderBy"] = value;
        }
    }
    private String OrderByDirection
    {
        get
        {
            object o = ViewState["Current_SortDirection"];
            if (o == null)
            {
                return " ASC ";
            }
            else
                return o.ToString();
        }
        set
        {
            ViewState["Current_SortDirection"] = value;
        }
    }
    private Int32 pageIndex
    {
        get
        {
            //object o = ViewState["Current_PageIndex"];
            //if (o == null)
            //{
            //    return 1;
            //}
            //else
            //    return Int32.Parse(o.ToString());
            return pagerMain.CurrentPageIndex;
        }
        set
        {
            //ViewState["Current_PageIndex"] = value;
            pagerMain.CurrentPageIndex = value;
        }
    }
    private Int32 pageSize
    {
        get
        {
            //object o = ViewState["Current_PageSize"];
            //if (o == null)
            //{
            //    return 10;
            //}
            //else
            //    return Int32.Parse(o.ToString());
            return pagerMain.PageSize;
        }
        set
        {
            //ViewState["Current_PageSize"] = value;
            pagerMain.PageSize = value;
        }
    }
    private Int32 pageCount
    {
        get
        {
            object o = ViewState["Current_PageCount"];
            if (o == null)
            {
                return 1;
            }
            else
                return Int32.Parse(o.ToString());
        }
        set
        {
            ViewState["Current_PageCount"] = value;
        }
    }

    #endregion
    protected void Page_Load(object sender, EventArgs e)
    {
       
        if (!IsPostBack)
        {
            BindData();
        }

    }
    #region GridView actions
    private void BindData()
    {
        int t = 0;
        grdResult.DataKeyNames = new String[] { _Entity.Persistence.Config(_Entity).IdentityFieldName };
        grdResult.DataSource = Orion.Entity2.EntityControl.Select(_Entity, pageIndex, pageSize, out t, Filter, String.IsNullOrEmpty(OrderBy) ? "" : OrderBy + OrderByDirection);
        grdResult.DataBind();

        pagerMain.RecordCount = t;


    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        EntitySearch es = new EntitySearch(_Entity);
        if (!String.IsNullOrEmpty(txtUserName_Search.Text))
        {
            es.AddSearchCodition("UserName", txtUserName_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtUserNo_Search.Text))
        {
            es.AddSearchCodition("UserNo", txtUserNo_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtName_Search.Text))
        {
            es.AddSearchCodition("Name", txtName_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtDepartment_Search.Text))
        {
            es.AddSearchCodition("Department", txtDepartment_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtPassword_Search.Text))
        {
            es.AddSearchCodition("Password", txtPassword_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtIsActive_Search.Text))
        {
            es.AddSearchCodition("IsActive", txtIsActive_Search.Text, "=");
        }

        if (!String.IsNullOrEmpty(txtExpirtationDate_Search.Text))
        {
            es.AddSearchCodition("ExpirtationDate", txtExpirtationDate_Search.Text, "=");
        }


        Filter = es.Where;
        BindData();

    }
    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtUserName_Search.Text = "";
        txtUserNo_Search.Text = "";
        txtName_Search.Text = "";
        txtDepartment_Search.Text = "";
        txtPassword_Search.Text = "";
        txtIsActive_Search.Text = "";
        txtExpirtationDate_Search.Text = "";

    }
    protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPageSize.SelectedIndex > -1)
        {
            pageIndex = 1;
            pageSize = Int32.Parse(ddlPageSize.SelectedValue);
            BindData();
        }

    }
    protected void grdResult_Sorting(object sender, GridViewSortEventArgs e)
    {

        if (OrderBy == e.SortExpression)
            if (OrderByDirection == " ASC ")
                OrderByDirection = " DESC ";
            else
                OrderByDirection = " ASC ";
        else
        {
            OrderByDirection = " ASC ";
            OrderBy = e.SortExpression;
        }
        BindData();
    }

    #endregion
    #region Detail actions
    protected void grdResult_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id = Int32.Parse(grdResult.SelectedDataKey.Value.ToString());
        Response.Redirect("UserEdit.aspx?id="+id.ToString());
    }
    #endregion

    protected void pagerMain_PageChanged(object sender, EventArgs e)
    {
        BindData();
    }
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        Response.Redirect("UserEdit.aspx");
    }
}

