
namespace Demo.WindowsForms
{
   partial class MainForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if(disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnGoTo = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxGeo = new System.Windows.Forms.TextBox();
            this.textBoxLng = new System.Windows.Forms.TextBox();
            this.textBoxLat = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBoxProgress = new System.Windows.Forms.GroupBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBoxUseRouteCache = new System.Windows.Forms.CheckBox();
            this.btnCachePrefetch = new System.Windows.Forms.Button();
            this.btnCacheClear = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.radioButtonPerf = new System.Windows.Forms.RadioButton();
            this.radioButtonVehicle = new System.Windows.Forms.RadioButton();
            this.radioButtonNone = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnAddRoute = new System.Windows.Forms.Button();
            this.btnSetEnd = new System.Windows.Forms.Button();
            this.btnSetStart = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxPlacemarkInfo = new System.Windows.Forms.CheckBox();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnZoomCenter = new System.Windows.Forms.Button();
            this.btnAddMarker = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxTileHost = new System.Windows.Forms.CheckBox();
            this.checkBoxCurrentMarker = new System.Windows.Forms.CheckBox();
            this.checkBoxCanDrag = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.checkBoxDebug = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.comboBoxMapType = new System.Windows.Forms.ComboBox();
            this.MainMap = new GMap.NET.WindowsForms.GMapControl();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBoxProgress.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnReload);
            this.groupBox1.Controls.Add(this.btnGoTo);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxGeo);
            this.groupBox1.Controls.Add(this.textBoxLng);
            this.groupBox1.Controls.Add(this.textBoxLat);
            this.groupBox1.Location = new System.Drawing.Point(14, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(203, 126);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Coordinates";
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(94, 97);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(79, 22);
            this.btnReload.TabIndex = 7;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnGoTo
            // 
            this.btnGoTo.Location = new System.Drawing.Point(9, 97);
            this.btnGoTo.Name = "btnGoTo";
            this.btnGoTo.Size = new System.Drawing.Size(79, 22);
            this.btnGoTo.TabIndex = 6;
            this.btnGoTo.Text = "Go To";
            this.btnGoTo.UseVisualStyleBackColor = true;
            this.btnGoTo.Click += new System.EventHandler(this.btnGoTo_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(163, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Geo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(163, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Lng";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(163, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Lat";
            // 
            // textBoxGeo
            // 
            this.textBoxGeo.Location = new System.Drawing.Point(9, 71);
            this.textBoxGeo.Name = "textBoxGeo";
            this.textBoxGeo.Size = new System.Drawing.Size(154, 20);
            this.textBoxGeo.TabIndex = 2;
            this.textBoxGeo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGeo_KeyPress);
            // 
            // textBoxLng
            // 
            this.textBoxLng.Location = new System.Drawing.Point(9, 45);
            this.textBoxLng.Name = "textBoxLng";
            this.textBoxLng.Size = new System.Drawing.Size(154, 20);
            this.textBoxLng.TabIndex = 1;
            // 
            // textBoxLat
            // 
            this.textBoxLat.Location = new System.Drawing.Point(9, 19);
            this.textBoxLat.Name = "textBoxLat";
            this.textBoxLat.Size = new System.Drawing.Size(154, 20);
            this.textBoxLat.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBoxProgress);
            this.panel2.Controls.Add(this.groupBox6);
            this.panel2.Controls.Add(this.groupBox5);
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Controls.Add(this.groupBox3);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(869, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(233, 718);
            this.panel2.TabIndex = 3;
            // 
            // groupBoxProgress
            // 
            this.groupBoxProgress.Controls.Add(this.progressBar1);
            this.groupBoxProgress.Location = new System.Drawing.Point(14, 647);
            this.groupBoxProgress.Name = "groupBoxProgress";
            this.groupBoxProgress.Size = new System.Drawing.Size(203, 59);
            this.groupBoxProgress.TabIndex = 5;
            this.groupBoxProgress.TabStop = false;
            this.groupBoxProgress.Text = "Progress";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(6, 21);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(167, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBox2);
            this.groupBox6.Controls.Add(this.checkBoxUseRouteCache);
            this.groupBox6.Controls.Add(this.btnCachePrefetch);
            this.groupBox6.Controls.Add(this.btnCacheClear);
            this.groupBox6.Controls.Add(this.btnImport);
            this.groupBox6.Controls.Add(this.btnExport);
            this.groupBox6.Location = new System.Drawing.Point(14, 549);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(203, 92);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Cache";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(107, 19);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(78, 17);
            this.checkBox2.TabIndex = 12;
            this.checkBox2.Text = "Geocoding";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBoxUseRouteCache
            // 
            this.checkBoxUseRouteCache.AutoSize = true;
            this.checkBoxUseRouteCache.Checked = true;
            this.checkBoxUseRouteCache.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUseRouteCache.Location = new System.Drawing.Point(15, 19);
            this.checkBoxUseRouteCache.Name = "checkBoxUseRouteCache";
            this.checkBoxUseRouteCache.Size = new System.Drawing.Size(63, 17);
            this.checkBoxUseRouteCache.TabIndex = 11;
            this.checkBoxUseRouteCache.Text = "Routing";
            this.checkBoxUseRouteCache.UseVisualStyleBackColor = true;
            // 
            // btnCachePrefetch
            // 
            this.btnCachePrefetch.Location = new System.Drawing.Point(94, 64);
            this.btnCachePrefetch.Name = "btnCachePrefetch";
            this.btnCachePrefetch.Size = new System.Drawing.Size(79, 22);
            this.btnCachePrefetch.TabIndex = 10;
            this.btnCachePrefetch.Text = "Prefetch";
            this.btnCachePrefetch.UseVisualStyleBackColor = true;
            this.btnCachePrefetch.Click += new System.EventHandler(this.btnCachePrefetch_Click);
            // 
            // btnCacheClear
            // 
            this.btnCacheClear.Location = new System.Drawing.Point(9, 64);
            this.btnCacheClear.Name = "btnCacheClear";
            this.btnCacheClear.Size = new System.Drawing.Size(79, 22);
            this.btnCacheClear.TabIndex = 9;
            this.btnCacheClear.Text = "Clear";
            this.btnCacheClear.UseVisualStyleBackColor = true;
            this.btnCacheClear.Click += new System.EventHandler(this.btnCacheClear_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(94, 39);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(79, 22);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(9, 39);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(79, 22);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.radioButtonPerf);
            this.groupBox5.Controls.Add(this.radioButtonVehicle);
            this.groupBox5.Controls.Add(this.radioButtonNone);
            this.groupBox5.Location = new System.Drawing.Point(14, 475);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(203, 68);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "RealTime";
            // 
            // radioButtonPerf
            // 
            this.radioButtonPerf.AutoSize = true;
            this.radioButtonPerf.Location = new System.Drawing.Point(100, 21);
            this.radioButtonPerf.Name = "radioButtonPerf";
            this.radioButtonPerf.Size = new System.Drawing.Size(85, 17);
            this.radioButtonPerf.TabIndex = 2;
            this.radioButtonPerf.Text = "Performance";
            this.radioButtonPerf.UseVisualStyleBackColor = true;
            this.radioButtonPerf.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
            // 
            // radioButtonVehicle
            // 
            this.radioButtonVehicle.AutoSize = true;
            this.radioButtonVehicle.Location = new System.Drawing.Point(25, 44);
            this.radioButtonVehicle.Name = "radioButtonVehicle";
            this.radioButtonVehicle.Size = new System.Drawing.Size(70, 17);
            this.radioButtonVehicle.TabIndex = 1;
            this.radioButtonVehicle.Text = "Transport";
            this.radioButtonVehicle.UseVisualStyleBackColor = true;
            this.radioButtonVehicle.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
            // 
            // radioButtonNone
            // 
            this.radioButtonNone.AutoSize = true;
            this.radioButtonNone.Checked = true;
            this.radioButtonNone.Location = new System.Drawing.Point(25, 21);
            this.radioButtonNone.Name = "radioButtonNone";
            this.radioButtonNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonNone.TabIndex = 0;
            this.radioButtonNone.TabStop = true;
            this.radioButtonNone.Text = "None";
            this.radioButtonNone.UseVisualStyleBackColor = true;
            this.radioButtonNone.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnAddRoute);
            this.groupBox4.Controls.Add(this.btnSetEnd);
            this.groupBox4.Controls.Add(this.btnSetStart);
            this.groupBox4.Location = new System.Drawing.Point(14, 386);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(203, 83);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Routes";
            // 
            // btnAddRoute
            // 
            this.btnAddRoute.Location = new System.Drawing.Point(9, 47);
            this.btnAddRoute.Name = "btnAddRoute";
            this.btnAddRoute.Size = new System.Drawing.Size(164, 22);
            this.btnAddRoute.TabIndex = 6;
            this.btnAddRoute.Text = "Add Route";
            this.btnAddRoute.UseVisualStyleBackColor = true;
            this.btnAddRoute.Click += new System.EventHandler(this.btnAddRoute_Click);
            // 
            // btnSetEnd
            // 
            this.btnSetEnd.Location = new System.Drawing.Point(94, 19);
            this.btnSetEnd.Name = "btnSetEnd";
            this.btnSetEnd.Size = new System.Drawing.Size(79, 22);
            this.btnSetEnd.TabIndex = 5;
            this.btnSetEnd.Text = "Set End";
            this.btnSetEnd.UseVisualStyleBackColor = true;
            this.btnSetEnd.Click += new System.EventHandler(this.btnSetEnd_Click);
            // 
            // btnSetStart
            // 
            this.btnSetStart.Location = new System.Drawing.Point(9, 19);
            this.btnSetStart.Name = "btnSetStart";
            this.btnSetStart.Size = new System.Drawing.Size(79, 22);
            this.btnSetStart.TabIndex = 4;
            this.btnSetStart.Text = "SetStart";
            this.btnSetStart.UseVisualStyleBackColor = true;
            this.btnSetStart.Click += new System.EventHandler(this.btnSetStart_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxPlacemarkInfo);
            this.groupBox3.Controls.Add(this.btnClearAll);
            this.groupBox3.Controls.Add(this.btnZoomCenter);
            this.groupBox3.Controls.Add(this.btnAddMarker);
            this.groupBox3.Location = new System.Drawing.Point(14, 301);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(203, 79);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Markers";
            // 
            // checkBoxPlacemarkInfo
            // 
            this.checkBoxPlacemarkInfo.AutoSize = true;
            this.checkBoxPlacemarkInfo.Location = new System.Drawing.Point(99, 22);
            this.checkBoxPlacemarkInfo.Name = "checkBoxPlacemarkInfo";
            this.checkBoxPlacemarkInfo.Size = new System.Drawing.Size(74, 17);
            this.checkBoxPlacemarkInfo.TabIndex = 3;
            this.checkBoxPlacemarkInfo.Text = "Place Info";
            this.checkBoxPlacemarkInfo.UseVisualStyleBackColor = true;
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(94, 46);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(79, 22);
            this.btnClearAll.TabIndex = 2;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // btnZoomCenter
            // 
            this.btnZoomCenter.Location = new System.Drawing.Point(9, 47);
            this.btnZoomCenter.Name = "btnZoomCenter";
            this.btnZoomCenter.Size = new System.Drawing.Size(79, 22);
            this.btnZoomCenter.TabIndex = 1;
            this.btnZoomCenter.Text = "Zoom Center";
            this.btnZoomCenter.UseVisualStyleBackColor = true;
            this.btnZoomCenter.Click += new System.EventHandler(this.btnZoomCenter_Click);
            // 
            // btnAddMarker
            // 
            this.btnAddMarker.Location = new System.Drawing.Point(9, 18);
            this.btnAddMarker.Name = "btnAddMarker";
            this.btnAddMarker.Size = new System.Drawing.Size(79, 22);
            this.btnAddMarker.TabIndex = 0;
            this.btnAddMarker.Text = "Add Marker";
            this.btnAddMarker.UseVisualStyleBackColor = true;
            this.btnAddMarker.Click += new System.EventHandler(this.btnAddMarker_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxTileHost);
            this.groupBox2.Controls.Add(this.checkBoxCurrentMarker);
            this.groupBox2.Controls.Add(this.checkBoxCanDrag);
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Controls.Add(this.checkBoxDebug);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.comboBoxMode);
            this.groupBox2.Controls.Add(this.comboBoxMapType);
            this.groupBox2.Location = new System.Drawing.Point(14, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(203, 151);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Gmap";
            // 
            // checkBoxTileHost
            // 
            this.checkBoxTileHost.AutoSize = true;
            this.checkBoxTileHost.Location = new System.Drawing.Point(9, 123);
            this.checkBoxTileHost.Name = "checkBoxTileHost";
            this.checkBoxTileHost.Size = new System.Drawing.Size(68, 17);
            this.checkBoxTileHost.TabIndex = 7;
            this.checkBoxTileHost.Text = "Tile Host";
            this.checkBoxTileHost.UseVisualStyleBackColor = true;
            this.checkBoxTileHost.CheckedChanged += new System.EventHandler(this.checkBoxTileHost_CheckedChanged);
            // 
            // checkBoxCurrentMarker
            // 
            this.checkBoxCurrentMarker.AutoSize = true;
            this.checkBoxCurrentMarker.Checked = true;
            this.checkBoxCurrentMarker.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCurrentMarker.Location = new System.Drawing.Point(94, 77);
            this.checkBoxCurrentMarker.Name = "checkBoxCurrentMarker";
            this.checkBoxCurrentMarker.Size = new System.Drawing.Size(96, 17);
            this.checkBoxCurrentMarker.TabIndex = 6;
            this.checkBoxCurrentMarker.Text = "Current Marker";
            this.checkBoxCurrentMarker.UseVisualStyleBackColor = true;
            this.checkBoxCurrentMarker.CheckedChanged += new System.EventHandler(this.checkBoxCurrentMarker_CheckedChanged);
            // 
            // checkBoxCanDrag
            // 
            this.checkBoxCanDrag.AutoSize = true;
            this.checkBoxCanDrag.Checked = true;
            this.checkBoxCanDrag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCanDrag.Location = new System.Drawing.Point(9, 100);
            this.checkBoxCanDrag.Name = "checkBoxCanDrag";
            this.checkBoxCanDrag.Size = new System.Drawing.Size(73, 17);
            this.checkBoxCanDrag.TabIndex = 5;
            this.checkBoxCanDrag.Text = "Drag Map";
            this.checkBoxCanDrag.UseVisualStyleBackColor = true;
            this.checkBoxCanDrag.CheckedChanged += new System.EventHandler(this.checkBoxCanDrag_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(94, 109);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(79, 22);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // checkBoxDebug
            // 
            this.checkBoxDebug.AutoSize = true;
            this.checkBoxDebug.Checked = true;
            this.checkBoxDebug.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDebug.Location = new System.Drawing.Point(9, 77);
            this.checkBoxDebug.Name = "checkBoxDebug";
            this.checkBoxDebug.Size = new System.Drawing.Size(45, 17);
            this.checkBoxDebug.TabIndex = 4;
            this.checkBoxDebug.Text = "Grid";
            this.checkBoxDebug.UseVisualStyleBackColor = true;
            this.checkBoxDebug.CheckedChanged += new System.EventHandler(this.checkBoxDebug_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(162, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Mode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(160, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Type";
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(9, 46);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(151, 21);
            this.comboBoxMode.TabIndex = 1;
            this.comboBoxMode.DropDownClosed += new System.EventHandler(this.comboBoxMode_DropDownClosed);
            // 
            // comboBoxMapType
            // 
            this.comboBoxMapType.FormattingEnabled = true;
            this.comboBoxMapType.Location = new System.Drawing.Point(9, 19);
            this.comboBoxMapType.Name = "comboBoxMapType";
            this.comboBoxMapType.Size = new System.Drawing.Size(151, 21);
            this.comboBoxMapType.TabIndex = 0;
            this.comboBoxMapType.DropDownClosed += new System.EventHandler(this.comboBoxMapType_DropDownClosed);
            // 
            // MainMap
            // 
            this.MainMap.Bearing = 0F;
            this.MainMap.CanDragMap = true;
            this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainMap.EmptyTileColor = System.Drawing.Color.Navy;
            this.MainMap.GrayScaleMode = false;
            this.MainMap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.MainMap.LevelsKeepInMemory = 5;
            this.MainMap.Location = new System.Drawing.Point(0, 0);
            this.MainMap.MarkersEnabled = true;
            this.MainMap.MaxZoom = 2;
            this.MainMap.MinZoom = 2;
            this.MainMap.MouseWheelZoomEnabled = true;
            this.MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.MainMap.Name = "MainMap";
            this.MainMap.NegativeMode = false;
            this.MainMap.PolygonsEnabled = true;
            this.MainMap.RetryLoadTile = 0;
            this.MainMap.RoutesEnabled = true;
            this.MainMap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.MainMap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.MainMap.ShowTileGridLines = false;
            this.MainMap.Size = new System.Drawing.Size(869, 718);
            this.MainMap.TabIndex = 4;
            this.MainMap.Zoom = 0D;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1102, 718);
            this.Controls.Add(this.MainMap);
            this.Controls.Add(this.panel2);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(554, 106);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GMap.NET - Great Maps for Windows Forms";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBoxProgress.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

      }

      #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBoxLat;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxGeo;
        private System.Windows.Forms.TextBox textBoxLng;
        public GMap.NET.WindowsForms.GMapControl MainMap;
        private System.Windows.Forms.ComboBox comboBoxMapType;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxDebug;
        private System.Windows.Forms.Button btnAddMarker;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnZoomCenter;
        private System.Windows.Forms.CheckBox checkBoxPlacemarkInfo;
        private System.Windows.Forms.Button btnAddRoute;
        private System.Windows.Forms.Button btnSetEnd;
        private System.Windows.Forms.Button btnSetStart;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton radioButtonPerf;
        private System.Windows.Forms.RadioButton radioButtonVehicle;
        private System.Windows.Forms.RadioButton radioButtonNone;
        private System.Windows.Forms.GroupBox groupBoxProgress;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnCachePrefetch;
        private System.Windows.Forms.Button btnCacheClear;
        private System.Windows.Forms.Button btnGoTo;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBoxUseRouteCache;
        private System.Windows.Forms.CheckBox checkBoxCanDrag;
        private System.Windows.Forms.CheckBox checkBoxCurrentMarker;
        private System.Windows.Forms.CheckBox checkBoxTileHost;
    }
}

