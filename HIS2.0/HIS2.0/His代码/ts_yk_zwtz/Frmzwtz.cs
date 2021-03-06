using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using YpClass;
using TrasenClasses.GeneralClasses;
using Trasen.Controls;
using TrasenClasses.DatabaseAccess;
using System.Collections;

namespace ts_yk_zwtz
{
    public partial class Frmzwtz : Form
    {
        DataTable tbmx;
        DataTable tbT;
        int _createId;
        DataTable tbError;
        private int rowIndex = -1;
        private string rowName;
        public Frmzwtz()
        {
            InitializeComponent();
        }


        private void Frmzwtz_Load(object sender, EventArgs e)
        {
            this.dtp1.Value = DateManager.ServerDateTimeByDBType(InstanceForm.BDatabase);
            this.dtp2.Value = DateManager.ServerDateTimeByDBType(InstanceForm.BDatabase);
            Yp.AddcmbCk(true, InstanceForm.BCurrentDept.DeptId, cmbck, InstanceForm.BDatabase);

            DataTable tbDrug = GetDrugInfo();
            InitTxtGgv(lbltxtYpmc, tbDrug);
            InitTxtGgv(lbltxtYpmc1, tbDrug);
            InitTxtGgv(labelTextBox1, GetGrugDept());
            _createId = InstanceForm.BCurrentUser.EmployeeId;
            lbltxtYpmc.Enabled = false;
            lbltxtYpmc1.Enabled = false;

            labelTextBox1.Text = cmbck.Text.Trim();
            labelTextBox1.SelectedValue = Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0"));
            CreateErrorTable();

            this.dgvMaster.DataSource = tbError;
            this.dgvMaster.Columns["id"].Visible = false;

            labelTextBox1.Enabled = false;
        }
        /// <summary>
        /// 弹出输入框，用于输入供货商
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextKeyUp(object sender, KeyEventArgs e)//KeyEventArgs
        {
            int nkey = Convert.ToInt32(e.KeyCode);
            Control control = (Control)sender;

            if (control.Text.Trim() == "")
            {
                control.Text = ""; control.Tag = "0";
            }

            if ((nkey >= 65 && nkey <= 90) || (nkey >= 48 && nkey <= 57) || (nkey >= 96 && nkey <= 105) || nkey == 8 || nkey == 32 || nkey == 46 || (nkey == 13 && (Convert.ToString(control.Tag) == "0" || Convert.ToString(control.Tag) == ""))) { }
            else { return; }

            try
            {
                Point point = new Point(this.Location.X + control.Location.X, this.Location.Y + control.Location.Y + control.Height * 3);
                switch (control.TabIndex)
                {
                    case 0:
                    case 29:
                        if (nkey == 13 && (control.Tag.ToString() != "" && control.Tag.ToString() != "0")) return;
                        Yp.frmShowCard(sender, ShowCardType.供货单位, 0, point, 0, InstanceForm.BDatabase);
                        if (Convertor.IsNull(control.Tag, "0") != "0") this.SelectNextControl((Control)sender, true, false, true, true);
                        break;
                }
            }
            catch (System.Exception err)
            {
                MessageBox.Show("发生错误" + err.Message);
            }
        }

        private void butref_Click(object sender, EventArgs e)
        {
            if (chkghdw.Checked == false && chkfph.Checked == false &&
                chkshdh.Checked == false &&
                chkdjsj.Checked == false &&
                chkypmc.Checked == false &&
                chkdjh.Checked == false)
            { MessageBox.Show("查询的记录范围太大，请重新选择查询条件"); return; }
            if (txtghdw.Text.Trim() == "" && txtghdw.Enabled == true) { MessageBox.Show("请输入供货单位"); return; }
            if (txtfph.Text.Trim() == "" && txtfph.Enabled == true) { MessageBox.Show("请输入发票号"); return; }
            if (txtshdh.Text.Trim() == "" && txtshdh.Enabled == true) { MessageBox.Show("请输入送货单号"); return; }
            if (chkypmc.Checked && lbltxtYpmc.Text.Trim() == "") { MessageBox.Show("请输入药品名称"); return; }

            if (!YpConfig.是否药库(Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")), InstanceForm.BDatabase))
            {
                MessageBox.Show("当前科室无帐务调整功能");
            }
            try
            {
                this.Cursor = PubStaticFun.WaitCursor();
                tbError.Clear();
                tbError.AcceptChanges();
                if (tbT != null)
                {
                    tbT.Clear();
                    tbT.AcceptChanges();
                }

                this.splitContainer2.Panel2Collapsed = true;
                tbmx = GetDjMx();
                tbmx.TableName = "Tb";
                FunBase.AddRowtNo(tbmx);
                #region
                //if (!tbmx.Columns.Contains("调整价格"))
                //{
                //    tbmx.Columns.Add("调整价格").SetOrdinal(5);
                //}
                //if (!tbmx.Columns.Contains("调整数量"))
                //{
                //    tbmx.Columns.Add("调整数量").SetOrdinal(6);
                //}
                //if (!tbmx.Columns.Contains("调整金额"))
                //{
                //    tbmx.Columns.Add("调整金额").SetOrdinal(7);
                //}
                #endregion
                this.dgvDetail.DataSource = tbmx;

                foreach (DataGridViewColumn col in this.dgvDetail.Columns)
                {
                    if (col.Name.Trim() == "序号" || col.Name.Trim() == "调整价格"
                        || col.Name.Trim() == "调整数量" || col.Name.Trim() == "冲减发票"
                        || col.Name.Trim() == "调整金额"
                        || col.Name.Trim() == "仓库名称" || col.Name.Trim() == "单据号"
                        || col.Name.Trim() == "单据日期" || col.Name.Trim() == "供货单位")
                    {
                        col.Frozen = true;
                    }
                    if (col.Name.Trim() != "调整价格" && col.Name.Trim() != "冲减发票") //col.Name.Trim() != "调整数量"
                    {
                        col.ReadOnly = true;
                    }
                    if (col.Name.Trim() == "kcid" || col.Name.Trim() == "扣率"
                        || col.Name.Trim() == "批发价" || col.Name.Trim() == "加成率" || col.Name.Trim() == "批发金额"
                        || col.Name.Trim() == "批发差额" || col.Name.Trim() == "cjid" ||
                        col.Name == "dwbl" || col.Name.Trim() == "id" || col.Name.Trim() == "基药"
                        || col.Name.Trim() == "原进价" || col.Name.Trim() == "djid"
                        || col.Name.Trim() == "WLDW" || col.Name.Trim() == "SHy"
                        || col.Name.Trim() == "djy" || col.Name.Trim() == "jgbm")
                    {
                        col.Visible = false;
                    }
                    if (col.Name.Trim() == "库存单位" || col.Name.Trim() == "单位")
                        col.Width = 70;
                }


                if (tbmx.Rows.Count > 0)
                {
                    dgvDetail.CurrentCell = this.dgvDetail.Rows[0].Cells["冲减发票"];//调整价格
                    dgvDetail.CurrentCell.Selected = true;
                    if (!dgvDetail.IsCurrentCellInEditMode)
                    {
                        dgvDetail.BeginEdit(true);
                    }
                }

                this.Cursor = Cursors.Default;
            }
            catch (System.Exception err)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(err.Message);
            }

        }

        private DataTable GetDjMx()
        {
            string sql = @"select 0 序号,dbo.fun_getdeptname(e.deptid) 仓库名称,e.DJH 单据号,
                            e.RQ 单据日期,dbo.FUN_YP_GHDW(e.WLDW) 供货单位,'0' 冲减发票,
                            0.00 as 调整价格,f.kcl as 调整数量,0.00 as 调整金额,
                            a.jhj 进价,a.pfj 批发价,a.lsj 零售价,a.yppch 批次号,ypsl 数量, a.ypdw 单位,f.kcl 库存数,dbo.fun_yp_ypdw(f.zxdw) 库存单位,
                            a.SHDH 送货单号,a.yppm 品名,a.ypgg 规格,a.ypspm 商品名,a.sccj 厂家,
                            a.kcid,a.ypph 批号,a.ypxq 效期,hwmc 库位,a.ypkl 扣率,
                            abs(cast(round((case when a.jhj<>0 then ((a.lsj-a.jhj)/a.jhj) else 0 end),3) as decimal(10,3))) 加成率,
                           jhje 进货金额,pfje 批发金额,lsje 零售金额,(lsje-jhje) 进零差额,
                            (lsje-pfje) 批零差额,a.shh 货号,pzwh 批准文号,
                            '' 库位 ,a.cjid,ydwbl dwbl,a.id ,(case when gjjbyw=1 then '是' else '' end)  基药, 
                            cast(a.fkbl*100 as decimal(15,2)) 付款比例, cast(a.jhje*a.fkbl as decimal(15,3)) 付款金额,
                            (select cast(yjhj/a.ydwbl as decimal(10,3)) from yk_kcph where id=a.kcid)  原进价,
                            e.fph 发票号,e.fprq 发票日期,
                            dbo.FUN_YP_YWY(e.jsr) 业务员,a.djid,e.WLDW,e.SHy,e.djy,e.jgbm,
                            cast((cast(e.djrq AS char(10))+' '+convert(nvarchar,e.DJSJ,108)) as datetime) as 登记时间, 
                            dbo.fun_getempname(e.djy) 登记员,shrq as 审核时间,dbo.fun_getempname(e.SHy) 审核员,e.bz 备注 
                            from  VI_YK_DJMX a inner join vi_yp_ypcd b on a.cjid=b.cjid 
                            inner join VI_YK_DJ e on e.ID=a.DJID and e.YWLX='001' and e.SHBZ=1
                            left join yp_hwsz c on b.ggid=c.ggid and a.deptid=c.deptid ";
            sql += " inner join yk_kcph f on a.deptid=f.deptid and a.kcid=f.id and a.cjid=f.cjid and ((f.bdelete=0 and f.kcl>0) or (f.bdelete=1 and f.kcl<>0))";//Modify By tany 2015-03-19 只要库存量大于0的，不要等于0的
            sql += " where  a.deptId=" + Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")) + "";
            if (this.chkghdw.Checked)
            {
                sql += " and e.wldw='" + Convert.ToInt32(this.txtghdw.Tag) + "'";
            }
            if (chkdjsj.Checked)
            {
                sql += "  and e.djrq>='" + dtp1.Value.ToShortDateString() + " 00:00:00 " + "'  and e.djrq<='" + dtp2.Value.ToShortDateString() + " 23:59:59" + "'";
            }
            if (chkdjh.Checked)
            {
                sql += " and  e.djh='" + Convert.ToInt64(Convertor.IsNull(txtdjh.Text, "0")) + "'";
            }
            if (this.chkfph.Checked)
            {
                sql += " and e.fph='" + txtfph.Text.Trim() + "'";
            }
            if (chkypmc.Checked)
            {
                sql += " and a.cjid='" + lbltxtYpmc.SelectedValue.ToString().Trim() + "'";
            }

            return InstanceForm.BDatabase.GetDataTable(sql);
        }

        private void chkghdw_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }

        private void chkfph_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }
        private void chkypmc_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }
        private void chkshdh_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }

        private void chkdjh_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }

        private void chkdjsj_CheckedChanged(object sender, EventArgs e)
        {
            this.txtghdw.Enabled = chkghdw.Checked == true ? true : false;
            this.txtshdh.Enabled = chkshdh.Checked == true ? true : false;
            this.txtfph.Enabled = chkfph.Checked == true ? true : false;
            this.dtp1.Enabled = chkdjsj.Checked == true ? true : false;
            this.dtp2.Enabled = chkdjsj.Checked == true ? true : false;
            this.txtdjh.Enabled = chkdjh.Checked == true ? true : false;
            this.lbltxtYpmc.Enabled = chkypmc.Checked == true ? true : false;
        }

        private void dgvMaster_DoubleClick(object sender, EventArgs e)
        {
            tbT = this.dgvDetail.DataSource as DataTable;
            tbmx = tbT;
            tbmx.AcceptChanges();
            this.dgvDetail.DataSource = tbmx;

            foreach (DataGridViewRow row in (IEnumerable)dgvDetail.Rows)
            {
                if ((row.Cells["id"].Value.ToString().Trim()) == dgvMaster.Rows[this.dgvMaster.CurrentCell.RowIndex].Cells["id"].Value.ToString().Trim())
                {
                    this.dgvDetail.EndEdit();
                    this.dgvDetail.CurrentCell = row.Cells["冲减发票"];
                    //this.dgvDetail.CurrentCell = row.Cells["调整价格"];
                    this.dgvDetail.FirstDisplayedScrollingRowIndex = row.Index;
                    this.dgvDetail.BeginEdit(true);
                    break;
                }
            }
        }
        private DataTable GetDrugInfo()
        {
            //string ssql = @"select distinct top 100  a.ggid,a.cjid,0 rowno,s_yppm,s_ypgg,s_sccj,s_ypdw,1 dwbl,
            //Modify By Tany 2015-01-28 不明白为什么要设置前100行？？
            string ssql = @"select a.ggid,a.cjid,0 rowno,s_yppm,s_ypgg,s_sccj,s_ypdw,1 dwbl,
                (case when scjj=0 or scjj is null then '' else cast(scjj as varchar(50)) end) scjj,
                a.mrjj,a.wbm,a.pym,
                pfj,lsj,shh,(case when GJJBYW=1 then '是' else '' end) 基药 from vi_yp_ypcd a inner join yp_ypbm b " +
                             " on a.ggid=b.ggid left join  yk_kcmx c on a.cjid=c.cjid  and c.deptid=" + Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")) + "  where cjbdelete=0  and a.n_ypzlx in(select ypzlx from yp_gllx where deptid=" + Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")) + ") ";

            return InstanceForm.BDatabase.GetDataTable(ssql);
        }

        private void InitTxtGgv(LabelTextBox labelTextBox, DataTable dt)
        {
            labelTextBox.ShowCardProperty[0].ShowCardDataSource = dt.Copy();
            labelTextBox.DisplayShowCardWhenActived = true;
        }

        private void dgvDetail_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex > 0)
            {
                System.Windows.Forms.DataGridView grid = (System.Windows.Forms.DataGridView)sender;
                grid.Rows[e.RowIndex].ErrorText = "";
                if (grid.Columns[e.ColumnIndex].Name.Trim() == "调整数量" || grid.Columns[e.ColumnIndex].Name.Trim() == "调整价格")
                {
                    decimal newDecimal = 0;
                    if (!decimal.TryParse(e.FormattedValue.ToString(), out newDecimal) || newDecimal < 0)
                    {
                        e.Cancel = true;
                        grid.Rows[e.RowIndex].ErrorText = "请输正数";
                        grid.CancelEdit();
                        //MessageBox.Show("请输正确金额数 !");
                        return;
                    }
                }
                else if (grid.Columns[e.ColumnIndex].Name.Trim() == "冲减发票")
                {
                    if (string.IsNullOrEmpty(e.FormattedValue.ToString().Trim()))
                    {
                        //MessageBox.Show("请输入冲减发票!");
                        //grid.Rows[e.RowIndex].ErrorText = "请输入冲减发票";
                        //e.Cancel = true;
                        //grid.CancelEdit();
                        grid.Rows[e.RowIndex].Cells["冲减发票"].Value = "0";
                        e.Cancel = false;
                        return;
                    }
                }
                else
                {
                    //转换失败，类型都有问题
                    e.Cancel = false;
                }
            }
        }
        private void dgvDetail_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void dgvDetail_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex > 0)
            {
                System.Windows.Forms.DataGridView grid = (System.Windows.Forms.DataGridView)sender;
                if (grid.Columns[e.ColumnIndex].Name.Trim() == "调整数量" || grid.Columns[e.ColumnIndex].Name.Trim() == "调整价格")
                {
                    grid.CurrentRow.Cells["调整金额"].Value = Convert.ToDecimal(Convertor.IsNull(grid.CurrentRow.Cells["调整数量"].Value, "0.0"))
                        * (Convert.ToDecimal(Convertor.IsNull(grid.CurrentRow.Cells["调整价格"].Value, "0.00")) - Convert.ToDecimal(Convertor.IsNull(grid.CurrentRow.Cells["进价"].Value, "0.00")));
                }
            }
        }

        private void dgvDetail_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvDetail_CellValueChanged(sender, e);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && !(ActiveControl is System.Windows.Forms.Button) && !(ActiveControl is LabelTextBox))//
            {

                if (this.dgvDetail.DataSource != null)
                {
                    if (dgvDetail.CurrentCell.OwningColumn.Name.Trim() == "调整数量")
                    {
                        if (this.dgvDetail.CurrentCell.RowIndex + 1 < this.dgvDetail.Rows.Count)
                        {
                            dgvDetail.CurrentCell = this.dgvDetail.Rows[this.dgvDetail.CurrentCell.RowIndex + 1].Cells["冲减发票"];//调整价格
                            dgvDetail.CurrentCell.Selected = true;
                        }
                        else
                        {
                            dgvDetail.CurrentCell = this.dgvDetail.Rows[0].Cells["冲减发票"];//调整价格
                            dgvDetail.CurrentCell.Selected = true;
                            //SendKeys.Send("{TAB}");
                        }
                    }
                    else
                    {
                        SendKeys.Send("{TAB}");
                    }
                    return true;
                }

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void CreateErrorTable()
        {
            tbError = new DataTable();
            DataColumn col1 = new DataColumn("单据号", typeof(System.Int32));
            DataColumn col2 = new DataColumn("供货单位", typeof(System.String));
            DataColumn col3 = new DataColumn("调整价格", typeof(System.Decimal));
            DataColumn col4 = new DataColumn("调整数量", typeof(System.Decimal));
            DataColumn col5 = new DataColumn("调整金额", typeof(System.Decimal));
            DataColumn col6 = new DataColumn("进价", typeof(System.Decimal));
            DataColumn col7 = new DataColumn("数量", typeof(System.Decimal));
            DataColumn col8 = new DataColumn("已调数", typeof(System.Decimal));
            DataColumn col9 = new DataColumn("可调数", typeof(System.Decimal));
            DataColumn col17 = new DataColumn("可调金额", typeof(System.Decimal));
            DataColumn col10 = new DataColumn("送货单号");
            DataColumn col11 = new DataColumn("品名");
            DataColumn col12 = new DataColumn("商品名");
            DataColumn col13 = new DataColumn("规格");
            DataColumn col14 = new DataColumn("厂家");
            DataColumn col15 = new DataColumn("批次号");
            DataColumn col16 = new DataColumn("id");
            tbError.Columns.AddRange(new DataColumn[] { col1, col2, col3, col4, col5, col6, col7, col8, col9, col17, col10, col11, col12, col13, col14, col15, col16 });

        }
        private void btnSave_Click(object sender, EventArgs e)
        {

            tbError.Clear();
            //保存
            DataTable tbTemp1 = (this.dgvDetail.DataSource as DataTable).Copy();
            DataTable tbTemp2;
            #region//查找发票号是否填写
            tbTemp1.DefaultView.RowFilter = "冲减发票<>'0' and (调整数量 <=0  or 调整价格 <=0)";
            string strTemp = "";
            DataTable tbTemp22 = tbTemp1.DefaultView.ToTable();
            if (tbTemp22.Rows.Count > 0)
            {
                for (int k = 0; k < tbTemp22.Rows.Count; k++)
                {
                    strTemp += tbTemp22.Rows[k]["冲减发票"].ToString().Trim() + "\t";
                }
                MessageBox.Show("请查看冲减发票的调整数量或者调整价格是否为0。冲减发票号为:\r\n\r\n" + strTemp);
                return;
            }
            #endregion

            #region //查看发票是否为空

            tbTemp1.DefaultView.RowFilter = "冲减发票='0' and (调整数量 >0  and 调整价格 >0)";
            DataTable tbTemp21 = tbTemp1.DefaultView.ToTable();
            if (tbTemp21.Rows.Count > 0)
            {
                for (int g = 0; g < tbTemp21.Rows.Count; g++)
                {
                    strTemp += tbTemp21.Rows[g]["单据号"].ToString().Trim() + "\t";
                }
                MessageBox.Show("请填写下面列表中的冲减发票,单据号为:\r\n\r\n" + strTemp);
                return;
            }
            #endregion

            tbTemp1.DefaultView.RowFilter = "调整数量 >0  and 调整价格 >0";
            tbTemp2 = tbTemp1.DefaultView.ToTable();
            if (tbTemp2.Rows.Count > 0)
            {
                RelationalDatabase db = InstanceForm.BDatabase;

                int rowIndx1 = this.dgvDetail.CurrentCell.RowIndex;
                string rowName1 = this.dgvDetail.CurrentCell.OwningColumn.Name.Trim();


                #region 验证数量与金额
                decimal sl1 = 0;
                decimal je1 = 0;
                DataRow[] rowerr;
                DataRow rowErr;
                DataTable tbValid = db.GetDataTable("select * from yk_cwtz_temp");
                for (int i = 0; i < tbTemp2.Rows.Count; i++)
                {
                    rowerr = tbValid.Select("djmxid='" + tbTemp2.Rows[i]["id"] + "'");
                    if (rowerr.Length > 0)
                    {
                        foreach (DataRow row1 in rowerr)
                        {
                            sl1 += Convert.ToDecimal(row1["tzsl"]);
                            je1 += Math.Abs(Convert.ToDecimal(row1["tzje"]));
                        }
                    }

                    //(je1 + Math.Abs(Convert.ToDecimal(tbTemp2.Rows[i]["调整金额"])) > Convert.ToDecimal(tbTemp2.Rows[i]["进货金额"])
                    if ((sl1 + Convert.ToDecimal(tbTemp2.Rows[i]["调整数量"])) > Convert.ToDecimal(tbTemp2.Rows[i]["数量"]))
                    {
                        rowErr = tbError.NewRow();
                        rowErr[0] = tbTemp2.Rows[i]["单据号"];
                        rowErr[1] = tbTemp2.Rows[i]["供货单位"];
                        rowErr[2] = tbTemp2.Rows[i]["调整价格"];
                        rowErr[3] = tbTemp2.Rows[i]["调整数量"];
                        rowErr[4] = tbTemp2.Rows[i]["调整金额"];
                        rowErr[5] = tbTemp2.Rows[i]["进价"];
                        rowErr[6] = tbTemp2.Rows[i]["数量"];
                        rowErr[7] = sl1;
                        rowErr[8] = Convert.ToDecimal(rowErr[6]) - sl1;
                        rowErr[9] = Convert.ToDecimal(tbTemp2.Rows[i]["进货金额"]) - je1;
                        rowErr[10] = tbTemp2.Rows[i]["送货单号"];
                        rowErr[11] = tbTemp2.Rows[i]["品名"];
                        rowErr[12] = tbTemp2.Rows[i]["商品名"];
                        rowErr[13] = tbTemp2.Rows[i]["规格"];
                        rowErr[14] = tbTemp2.Rows[i]["厂家"];
                        rowErr[15] = tbTemp2.Rows[i]["批次号"];
                        rowErr[16] = tbTemp2.Rows[i]["id"];
                        tbError.Rows.Add(rowErr);
                    }
                    sl1 = 0;
                    je1 = 0;
                }
                if (tbError.Rows.Count > 0)
                {
                    tbError.AcceptChanges();
                    MessageBox.Show("帐务调整失败，请修改相应调整金额与调整数量", "帐务调整提示");
                    this.splitContainer2.Panel2Collapsed = false;

                    this.dgvDetail.EndEdit();
                    tbT = this.dgvDetail.DataSource as DataTable;
                    tbmx = tbT;
                    tbmx.AcceptChanges();
                    this.dgvDetail.DataSource = tbmx;
                    this.dgvDetail.CurrentCell = this.dgvDetail.Rows[rowIndx1].Cells[rowName1];
                    this.dgvDetail.CurrentCell.Selected = true;
                    this.dgvDetail.BeginEdit(true);

                    return;
                }
                else
                {
                    this.splitContainer2.Panel2Collapsed = true;
                }
                #endregion

                string fph = string.Empty;
                decimal totalMoney = 0;
                int saveCount = tbTemp2.Rows.Count;
                List<string[]> verifList = new List<string[]>();
                for (int x = 0; x < saveCount; x++)
                {
                    totalMoney += tbTemp2.Rows[x]["调整金额"] != null && tbTemp2.Rows[x]["调整金额"] != DBNull.Value ? Convert.ToDecimal(tbTemp2.Rows[x]["调整金额"].ToString().Trim()) : 0;
                    fph = tbTemp2.Rows[x]["冲减发票"].ToString();
                    List<string[]> existenceList = verifList.FindAll(delegate(string[] arr)
                    {
                        return arr[0].ToString().Trim() == tbTemp2.Rows[x]["供货单位"].ToString().Trim();
                    });
                    if (existenceList == null || existenceList.Count == 0)
                        verifList.Add(new string[] { tbTemp2.Rows[x]["供货单位"].ToString().Trim(), tbTemp2.Rows[x]["冲减发票"].ToString().Trim() });
                    else
                    {
                        for (int p = 0; p < existenceList.Count; p++)
                        {
                            string[] currentItem = existenceList[p];
                            if (currentItem.Length > 0 && currentItem[1].ToString().Trim() != tbTemp2.Rows[x]["冲减发票"].ToString().Trim())
                            {
                                MessageBox.Show(string.Format("{0}中冲减发票必须一致.请修改!", currentItem[0]));
                                return;
                            }
                        }
                        //相同供货单位中 发票号都一致                       
                    }
                }

                //Modify By Tany 2015-03-19 检查各药房的库存情况，如果有库存提示退库
                string msg = "";
                for (int t = 0; t < saveCount; t++)
                {
                    object[] param = { tbTemp2.Rows[t]["cjid"].ToString(),
                                       tbTemp2.Rows[t]["批次号"].ToString(),
                                       tbTemp2.Rows[t]["批号"].ToString()};

                    string sql = string.Format(@"select a.*,b.NAME from Yf_KCPH a left join JC_DEPT_PROPERTY b on a.DEPTID =  b.DEPT_ID                                  
                                 where CJID = {0} and YPPCH = '{1}' and YPPH  = '{2}' and kcl<>0", param);
                    DataTable dt = db.GetDataTable(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        msg += string.Format("\r\n{0}[批次号:{1}][批号:{2}]在以下药房还有库存：\r\n",
                                tbTemp2.Rows[t]["商品名"].ToString(),
                                tbTemp2.Rows[t]["批次号"].ToString(),
                                tbTemp2.Rows[t]["批号"].ToString());
                        for (int k = 0; k < dt.Rows.Count; k++)
                        {
                            msg += string.Format("    {0} 库存：{1}\r\n",
                                dt.Rows[k]["NAME"].ToString(),
                                dt.Rows[k]["kcl"].ToString());
                        }
                    }
                }
                if (msg != "")
                {
                    MessageBox.Show(msg + "\r\n请先做退库后才能继续执行此操作！", "提示");
                    return;
                }

                decimal sum1 = totalMoney; //Convert.ToDecimal(Convertor.IsNull(tbTemp2.Compute("sum([调整金额])", "调整金额>0"), "0.00"));

                if (MessageBox.Show("此次帐务调整金额为:" + sum1 + " 是否继续进行帐务调整", "帐务调整提示", MessageBoxButtons.YesNo, MessageBoxIcon.None) == DialogResult.No)
                {
                    return;
                }

                this.Cursor = PubStaticFun.WaitCursor();

                long djh = 0;       //单据号
                string sdjh = "";
                Guid djid = Guid.Empty;	  //主表ID
                int err_code = 0;   //错误号
                string err_text = "";//借误文本
                int j = 0;

                yk_cwtz_temp yket = new yk_cwtz_temp(db);
                DateTime dtservices = DateManager.ServerDateTimeByDBType(db);
                try
                {
                    db.BeginTransaction();

                    #region 产生单据头
                    //产生单据号
                    djh = Yp.SeekNewDjh("012", Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")), db);
                    sdjh = Yp.SeekNewDjh_Str("012", Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")), db);
                    decimal sumjhje = Convert.ToDecimal(Convertor.IsNull(tbTemp2.Compute("sum([调整金额])", "true"), "0"));
                    //保存单据表头
                    Yk_dj_djmx.SaveDJ(Guid.Empty,
                        djh,
                        Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")),
                        "012",
                        Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")),
                        0,
                        dtservices.ToShortDateString(),
                        InstanceForm.BCurrentUser.EmployeeId,
                        dtservices.ToShortDateString(),
                        dtservices.ToLongTimeString(),
                        fph,
                        "",
                        "",
                        fph,
                        0,
                        0,
                        sumjhje,
                        0,
                        0,
                        sdjh,
                        out djid, out err_code, out err_text, InstanceForm._menuTag.Jgbm, db);
                    if (err_code != 0)
                    {
                        db.RollbackTransaction();
                        this.Cursor = Cursors.Default;
                        throw new System.Exception(err_text);
                    }
                    string ss = djid.ToString();

                    #endregion

                    foreach (DataRow row in tbTemp2.Rows)
                    {
                        j++;

                        #region 单据明细
                        Yk_dj_djmx.SaveDJMX(Guid.Empty,
                                djid,
                                Convert.ToInt32(row["cjid"]),
                                0,
                                Convert.ToString(row["货号"]),
                                Convert.ToString(row["品名"]),
                                Convert.ToString(row["商品名"]),
                                Convert.ToString(row["规格"]),
                                Convert.ToString(row["厂家"]),
                                Convert.ToString(row["批号"]),
                                Convert.ToString(row["效期"]),
                                0,
                                0,
                                0,
                                Convert.ToString(row["单位"]),
                                Yp.SeekYpdw(Convert.ToString(row["单位"]), db),
                                Convert.ToInt32(row["dwbl"]),
                                0,
                                0,
                                0,
                                Convert.ToDecimal(row["调整金额"]),
                                0,
                                0,
                                djh,
                                 Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0")),
                               "012",
                                "",
                                row["冲减发票"].ToString().Trim(), 0,
                                out err_code, out err_text, db, j,
                                row["批次号"].ToString(),
                                new Guid(row["kcid"].ToString()));

                        if (err_code != 0)
                        {
                            db.RollbackTransaction();
                            this.Cursor = Cursors.Default;
                            throw new System.Exception(err_text);
                        }
                        #endregion

                        #region 帐务调整明细数据
                        yket.djh = Convert.ToInt32(row["单据号"].ToString());
                        yket.Djid = new Guid(row["djid"].ToString());
                        yket.Djmxid = new Guid(row["id"].ToString());
                        yket.deptid = Convert.ToInt32(Convertor.IsNull(cmbck.SelectedValue, "0"));
                        yket.Kcid = new Guid(row["kcid"].ToString());
                        yket.cjid = Convert.ToInt32(Convertor.IsNull(row["cjid"].ToString(), "0"));
                        yket.wldw = Convert.ToInt32(row["WLDW"].ToString());
                        yket.yppch = row["批次号"].ToString();
                        yket.ywlx = "012";
                        yket.YPDW = Convertor.IsNull(row["单位"], "");
                        yket.RQ = Convert.ToDateTime(row["单据日期"]);
                        yket.SHDH = row["送货单号"].ToString().Trim();
                        yket.SHH = row["货号"].ToString();
                        yket.shr = Convert.ToInt32(row["SHy"].ToString().Trim());
                        yket.shrq = Convert.ToDateTime(row["审核时间"].ToString());
                        yket.djy = Convert.ToInt32(row["djy"].ToString().Trim());
                        yket.djrq = Convert.ToDateTime(row["登记时间"].ToString().Trim());
                        yket.FPH = row["发票号"].ToString().Trim();
                        yket.FPRQ = row["发票日期"].ToString().Trim();
                        yket.ypph = row["批号"].ToString().Trim();
                        yket.jgbm = Convert.ToInt64(row["jgbm"].ToString().Trim());
                        yket.xdjh = djh;
                        yket.xdjid = djid;

                        yket.xfph = row["冲减发票"].ToString().Trim();
                        yket.tzjg = Convert.ToDecimal(Convertor.IsNull(row["调整价格"], "0"));
                        yket.tzsl = Convert.ToDecimal(Convertor.IsNull(row["调整数量"], "0"));
                        yket.tzje = Convert.ToDecimal(Convertor.IsNull(row["调整金额"], "0"));

                        yket.cjr = _createId;
                        yket.cjrq = dtservices;
                        yket.Add(yket);
                        #endregion

                        string sql = string.Format("update YP_YPCJD set mrjj = {0} where cjid = {1}", Convert.ToDecimal(Convertor.IsNull(row["调整价格"], "0")), Convert.ToInt32(Convertor.IsNull(row["cjid"].ToString(), "0")));
                        db.DoCommand(sql);
                    }

                    //审核单据
                    Yk_dj_djmx.Shdj(djid, dtservices.ToString("yyyy-MM-dd HH:mm:ss"), db);


                    db.CommitTransaction();
                    MessageBox.Show("帐务调整成功", "帐务调整提示");

                    #region 重新绑定
                    this.dgvDetail.EndEdit();

                    tbT = this.dgvDetail.DataSource as DataTable;
                    tbmx = tbT;
                    tbmx.AcceptChanges();
                    this.dgvDetail.DataSource = tbmx;
                    this.dgvDetail.CurrentCell = this.dgvDetail.Rows[rowIndx1].Cells[rowName1];
                    this.dgvDetail.CurrentCell.Selected = true;
                    this.dgvDetail.BeginEdit(true);
                    #endregion

                    this.Cursor = Cursors.Default;
                }
                catch (Exception err)
                {
                    db.RollbackTransaction();
                    this.Cursor = Cursors.Default;
                    throw err;
                }
            }
            else
            {
                MessageBox.Show("调整金额为零，帐务调整失败", "信息提示");
            }
        }

        private void dgvDetail_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.dgvDetail.IsCurrentCellInEditMode
                && (this.dgvDetail.CurrentCell.OwningColumn.Name.Trim() == "调整价格"
                || this.dgvDetail.CurrentCell.OwningColumn.Name.Trim() == "调整数量")
                && this.dgvDetail.IsCurrentCellDirty)
            {
                if (this.dgvDetail.CurrentCell.OwningColumn.Name.Trim() == "调整数量"
                    || this.dgvDetail.CurrentCell.OwningColumn.Name.Trim() == "调整价格")
                {
                    this.dgvDetail.CurrentRow.Cells["调整金额"].Value = Convert.ToDecimal(Convertor.IsNull(dgvDetail.CurrentRow.Cells["调整数量"].Value, "0.0")) * Convert.ToDecimal(Convertor.IsNull(dgvDetail.CurrentRow.Cells["调整价格"].Value, "0.00"));
                    //this.dgvDetail.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void IsCheckChanged(object sender, EventArgs e)
        {
            this.txtghdw1.Enabled = chkghdw1.Checked == true ? true : false;
            this.txtshdh1.Enabled = chkshdh1.Checked == true ? true : false;
            this.txtfph1.Enabled = chkfph1.Checked == true ? true : false;
            this.dtp11.Enabled = chkdjsj1.Checked == true ? true : false;
            this.dtp21.Enabled = chkdjsj1.Checked == true ? true : false;
            this.txtdjh1.Enabled = chkdjh1.Checked == true ? true : false;
            this.lbltxtYpmc1.Enabled = chkypmc1.Checked == true ? true : false;
            this.txtxdjh1.Enabled = this.chkxdjh1.Checked == true ? true : false;
            this.txtxfph1.Enabled = this.chkxfph1.Checked == true ? true : false;
        }

        private DataTable GetGrugDept()
        {
            StringBuilder str = new StringBuilder();
            str.Append("select rtrim(ltrim(ksmc)) as name,deptid as id,dbo.GETPYWB(KSMC,0) as pym,dbo.GETPYWB(KSMC,1) as wbm from YP_YJKS");
            return InstanceForm.BDatabase.GetDataTable(str.ToString());
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            if (chkghdw1.Checked == false && chkfph1.Checked == false &&
               chkshdh1.Checked == false &&
               chkdjsj1.Checked == false &&
               chkypmc1.Checked == false &&
               chkdjh1.Checked == false &&
               chkxdjh1.Checked == false &&
               labelTextBox1.Enabled == false &&
               chkxfph1.Checked == false)
            {
                MessageBox.Show("查询的记录范围太大，请重新选择查询条件");
                return;
            }
            DataTable tb = Query();
            FunBase.AddRowtNo(tb);
            this.dgv1.DataSource = tb;
            foreach (DataGridViewColumn col in this.dgv1.Columns)
            {
                if (col.Name.Trim() == "cjid" || col.Name.Trim() == "deptid"
                    || col.Name.Trim() == "wldw" || col.Name.Trim() == "xdjid" || col.Name.Trim() == "id")
                {
                    col.Visible = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private DataTable Query()
        {
            StringBuilder str = new StringBuilder();
            str.Append(@"select 0 序号,dbo.fun_getdeptname(a.deptid) 仓库名称,
                        a.RQ 单据日期,dbo.FUN_YP_GHDW(a.WLDW) 供货单位,a.DJH  原单据号,a.xfph 冲减发票,
                        a.xdjh 现单据号, cast(a.tzjg as decimal(18,3)) 调整价格,CAST(a.tzsl as decimal(18,3)) 调整数量,
                        CAST(tzje as decimal(18,3)) 调整金额,a.YPDW 单位,a.yppch 批次号, b.JHJ 进价,b.YPSL 数量,b.YPDW 原单位,jhje 进货金额,
                        b.yppm 品名,b.ypspm 商品名,b.ypgg 规格,b.sccj 厂家,
                        b.PFJ 批发价,pfje 批发金额,LSJ 零售价,lsje 零售金额, (lsje-jhje) 进零差额, (lsje-pfje) 批零差额,
                        a.shh 货号,a.SHDH 送货单号,a.FPH 发票号,a.FPRQ 发票日期,
                        a.djrq 登记时间,dbo.fun_getempname(a.djy) 登记员,shrq as 审核时间,dbo.fun_getempname(a.shr) 审核员, 
                        dbo.fun_getempname(a.cjr) 帐务登记人,a.cjrq 帐务登记时间,a.deptid,a.cjid,a.wldw,a.xdjid,a.id    
                        from yk_cwtz_temp a 
                        inner join VI_YK_DJMX b on a.djmxid=b.ID where 1=1 ");
            if (labelTextBox1.SelectedValue != null) str.Append(" and a.deptid='" + labelTextBox1.SelectedValue.ToString() + "' ");
            if (this.chkghdw1.Checked)
            {
                str.Append(" and a.wldw='" + Convert.ToInt32(this.txtghdw1.Tag) + "'");
            }
            if (chkdjsj1.Checked)
            {
                str.Append("  and a.cjrq>='" + dtp11.Value.ToShortDateString() + " 00:00:00 " + "'  and a.cjrq<='" + dtp21.Value.ToShortDateString() + " 23:59:59" + "'");
            }
            if (chkdjh1.Checked)
            {
                str.Append(" and  a.djh='" + Convert.ToInt64(Convertor.IsNull(txtdjh1.Text, "0")) + "'");
            }
            if (this.chkfph1.Checked)
            {
                str.Append(" and a.fph='" + txtfph.Text.Trim() + "'");
            }
            if (chkypmc1.Checked)
            {
                str.Append(" and a.cjid='" + lbltxtYpmc1.SelectedValue.ToString().Trim() + "'");
            }
            if (chkxdjh1.Checked)
            {
                str.Append(" and a.xdjh='" + txtxdjh1.Text.Trim() + "'");
            }
            if (chkxfph1.Checked)
            {
                str.Append(" and a.xfph='" + txtxfph1.Text.Trim() + "'");
            }
            return InstanceForm.BDatabase.GetDataTable(str.ToString());
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(tabControl1.SelectedTab.Tag) == 0)
            {
                if (tbT == null) return;
                tbmx = tbT;
                tbmx.AcceptChanges();
                this.dgvDetail.DataSource = tbmx;
                //
                if (rowIndex >= 0)
                {
                    if (rowName.Trim() == "调整价格" || rowName.Trim() == "调整数量" || rowName.Trim() == "冲减发票")
                    {
                        this.dgvDetail.CurrentCell = this.dgvDetail.Rows[rowIndex].Cells[rowName];
                    }
                    else
                    {
                        this.dgvDetail.CurrentCell = this.dgvDetail.Rows[rowIndex].Cells["冲减发票"];
                    }
                    this.dgvDetail.BeginEdit(true);
                }
            }
            else
            {
                if (this.dgvDetail.DataSource == null) return;
                if ((this.dgvDetail.DataSource as DataTable).Rows.Count <= 0) return;
                rowIndex = this.dgvDetail.CurrentCell.RowIndex;
                rowName = this.dgvDetail.CurrentCell.OwningColumn.Name.Trim();
                tbT = this.dgvDetail.DataSource as DataTable;
            }
        }

        private void dgv1_DoubleClick(object sender, EventArgs e)
        {
            if (this.dgv1.SelectedCells.Count > 0)
            {
                if (IsValidYj())
                {
                    MessageBox.Show("该单据已被月结不能修改");
                    return;
                }
                int rowIndex2 = this.dgv1.CurrentCell.RowIndex;
                Frmyzje frm1 = new Frmyzje();
                frm1.Tag = this.dgv1.Rows[rowIndex2].Cells["id"].Value.ToString().Trim();
                frm1.t1.Text = this.dgv1.Rows[rowIndex2].Cells["现单据号"].Value.ToString().Trim();
                frm1.t1.Tag = this.dgv1.Rows[rowIndex2].Cells["xdjid"].Value;

                frm1.t2.Text = this.dgv1.Rows[rowIndex2].Cells["供货单位"].Value.ToString().Trim();
                frm1.t2.Tag = this.dgv1.Rows[rowIndex2].Cells["deptid"].Value.ToString().Trim();

                frm1.t3.Text = this.dgv1.Rows[rowIndex2].Cells["品名"].Value.ToString().Trim();
                frm1.t3.Tag = this.dgv1.Rows[rowIndex2].Cells["cjid"].Value.ToString().Trim();

                frm1.t4.Text = this.dgv1.Rows[rowIndex2].Cells["商品名"].Value.ToString().Trim();
                frm1.t5.Text = this.dgv1.Rows[rowIndex2].Cells["规格"].Value.ToString().Trim();
                frm1.t6.Text = this.dgv1.Rows[rowIndex2].Cells["厂家"].Value.ToString().Trim();
                frm1.t7.Text = this.dgv1.Rows[rowIndex2].Cells["进价"].Value.ToString().Trim();
                frm1.t8.Text = this.dgv1.Rows[rowIndex2].Cells["批次号"].Value.ToString().Trim();
                frm1.t9.Text = this.dgv1.Rows[rowIndex2].Cells["调整价格"].Value.ToString().Trim();

                frm1.t10.Text = this.dgv1.Rows[rowIndex2].Cells["调整数量"].Value.ToString().Trim();
                frm1.t10.Tag = this.dgv1.Rows[rowIndex2].Cells["数量"].Value.ToString().Trim();

                frm1.t11.Text = this.dgv1.Rows[rowIndex2].Cells["调整金额"].Value.ToString().Trim();
                frm1.t11.Tag = this.dgv1.Rows[rowIndex2].Cells["进货金额"].Value.ToString().Trim();

                frm1.t12.Text = this.dgv1.Rows[rowIndex2].Cells["冲减发票"].Value.ToString().Trim();

                if (frm1.ShowDialog() == DialogResult.OK)
                {

                    this.dgv1.Rows[rowIndex2].Cells["调整数量"].Value = Convert.ToDecimal(frm1.t10.Text.Trim());
                    this.dgv1.Rows[rowIndex2].Cells["调整价格"].Value = Convert.ToDecimal(frm1.t9.Text.Trim());
                    this.dgv1.Rows[rowIndex2].Cells["调整金额"].Value = Convert.ToDecimal(frm1.t11.Text.Trim());
                }
                frm1.Dispose();
            }
        }
        /// <summary>
        /// 是否月结
        /// </summary>
        private bool IsValidYj()
        {
            StringBuilder str = new StringBuilder();
            str.Append("select YJID from VI_YK_DJ where ID='" + this.dgv1.Rows[this.dgv1.CurrentCell.RowIndex].Cells["xdjid"].Value.ToString() + "'");
            object obj = InstanceForm.BDatabase.GetDataResult(str.ToString());
            if (obj != null && obj != System.DBNull.Value && obj.ToString().Trim().Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void dgvDetail_MouseLeave(object sender, EventArgs e)
        {
            //dgvDetail.CausesValidation = false;

        }

        private void dgvDetail_MouseEnter(object sender, EventArgs e)
        {
            // dgvDetail.CausesValidation = true;
        }

        private void dgvDetail_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int columnsindex = e.ColumnIndex;
            int rowindex = e.RowIndex;
            if (rowindex >= 0)
            {
                DataGridViewRow row = this.dgvDetail.Rows[rowindex];
                if (this.dgvDetail.Columns[columnsindex].Name == "冲减发票" ||
                    this.dgvDetail.Columns[columnsindex].Name == "调整价格" ||
                    this.dgvDetail.Columns[columnsindex].Name == "调整数量" ||
                    this.dgvDetail.Columns[columnsindex].Name == "调整金额")
                {
                    row.Cells[columnsindex].Style.BackColor = Color.LightPink;
                }
            }
        }
    }
}
