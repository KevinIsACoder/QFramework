using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark;

namespace SparkEditor.UI
{
	public class LuaScriptGenerator : ScriptGenerator
	{
		private string m_BaseViewName;

		public LuaScriptGenerator() : this("UIView") { }

		public LuaScriptGenerator(string baseViewName)
		{
			extension = "lua";
			m_BaseViewName = baseViewName;
		}

		public override void Generate(string file, List<KeyValuePair<string, Type>> properties, List<UIViewExporter.UISubViewCollection> tableViews, List<UIViewExporter.UISubViewCollection> viewStacks, string prefabPath)
		{
			string className = Path.GetFileNameWithoutExtension(file);

			StringBuilder builder = new StringBuilder();

			bool hasCache = (tableViews != null && tableViews.Count > 0) || (viewStacks != null && viewStacks.Count > 0);

			builder.AppendFormat("---@class {0} : {1}", className, m_BaseViewName).AppendLine();
			builder.AppendFormat("local {0} = class(\"{0}\", {1})", className, m_BaseViewName).AppendLine();

			builder.AppendLine();
			builder.AppendLine("-- Properties");
			builder.AppendFormat("{0}.static.prefabPath = \"{1}\"", className, prefabPath).AppendLine();
			if (hasCache) {
				builder.AppendFormat("{0}.mCachedViews = nil", className).AppendLine();
			}
			builder.AppendLine();
			builder.AppendFormat("function {0}:OnCreated()", className).AppendLine();
			builder.Append("\t").Append(m_BaseViewName).AppendLine(".OnCreated(self)");
			if (hasCache) {
				builder.Append("\t").AppendLine("self.mCachedViews = {}");
			}
			if (properties.Count > 0) {
				builder.Append("\t").Append("local coms = self.component:GetComponents(self.transform, true)").AppendLine();
				builder.Append("\t").Append("self.view = setmetatable({}, {").AppendLine();
				builder.Append("\t\t").Append("__index = function(t, k)").AppendLine();
				builder.Append("\t\t\t").Append("local com = nil").AppendLine();
				if (properties.Count > 0)
                {
					for (int i = 0; i < properties.Count; i++) {
						var kv = properties[i];
						builder.Append("\t\t\t").AppendFormat("{0}if k == \"{1}\" then com = coms:Get({2}){3}", i > 0 ? "else" : "", kv.Key.ToLowerFirst(), i, (i == properties.Count - 1) ? " end" : "").AppendLine();
					}
					builder.Append("\t\t\t").Append("rawset(t, k, com)").AppendLine();
				}
				builder.Append("\t\t\t").Append("return com").AppendLine();
				builder.Append("\t\t").Append("end").AppendLine();
				builder.Append("\t").Append("})").AppendLine();
			}
			builder.AppendLine("end");
			builder.AppendLine();

			if (hasCache) {
				builder.AppendFormat("function {0}:OnDestroyed()", className).AppendLine();
				builder.Append("\t").AppendLine("self.mCachedViews = nil");
				builder.Append("\t").Append(m_BaseViewName).AppendLine(".OnDestroyed(self)");
				builder.AppendLine("end");
				builder.AppendLine();
			}

			if (viewStacks != null && viewStacks.Count > 0) {
				builder.AppendFormat("function {0}:OnBeforeViewStackValueChanged(viewStack)", className).AppendLine();
				builder.Append("\t").AppendLine("local transform = viewStack.selectedValue");
				builder.Append("\t").AppendLine("if transform ~= nil and not self.mCachedViews[transform] then");
				builder.Append("\t\t").AppendLine("local index = viewStack.selectedIndex");
				int m = 0;
				for (int i = 0; i < viewStacks.Count; i++) {
					var collection = viewStacks[i];
					if (collection.views.Any((view) => view.properties.Count > 0)) {
						builder.Append("\t\t");
						if (m > 0) {
							builder.Append("else");
						}
						builder.AppendFormat("if viewStack == self.{0} then", collection.name.ToLowerFirst()).AppendLine();
						if (collection.views.Count > 1) {
							int n = 0;
							for (int j = 0; j < collection.views.Count; j++) {
								var view = collection.views[j];
								if (view.properties.Count > 0) {
									builder.Append("\t\t\t");
									if (n > 0) {
										builder.Append("else");
									}
									builder.AppendFormat("if index == {0} then", j).AppendLine();
									builder.Append("\t\t\t\t").Append("local coms = self.component:GetComponents(transform, true)").AppendLine();
									if (view.properties.Count > 0)
									{
										builder.Append("\t\t\t\t").Append("local mt = getmetatable(self.view)").AppendLine();
										builder.Append("\t\t\t\t").Append("setmetatable(self.view, {").AppendLine();
										builder.Append("\t\t\t\t\t").Append("__index = function(t, k)").AppendLine();
										builder.Append("\t\t\t\t\t\t").Append("local com = nil").AppendLine();
										for (int k = 0; k < view.properties.Count; k++)
										{
											var kv = view.properties[k];
											builder.Append("\t\t\t\t\t\t").AppendFormat("{0}if k == \"{1}\" then com = coms:Get({2}){3}", k > 0 ? "else" : "", kv.Key.ToLowerFirst(), k, (k == view.properties.Count - 1) ? " end" : "").AppendLine();
										}
										builder.Append("\t\t\t\t\t\t").Append("if com == nil then return mt.__index(self.view, k) end").AppendLine();
										builder.Append("\t\t\t\t\t\t").Append("rawset(t, k, com)").AppendLine();
										builder.Append("\t\t\t\t\t\t").Append("return com").AppendLine();
										builder.Append("\t\t\t\t\t").Append("end").AppendLine();
										builder.Append("\t\t\t\t").Append("})").AppendLine();
									}
									n++;
								}
							}
							if (n > 0) {
								builder.Append("\t\t\t").AppendLine("end");
							}
						} else {
							var view = collection.views[0];
							builder.Append("\t\t\t").Append("local coms = self.component:GetComponents(transform, true)").AppendLine();
							if (view.properties.Count > 0)
							{
								builder.Append("\t\t\t").Append("local mt = getmetatable(self.view)").AppendLine();
								builder.Append("\t\t\t").Append("setmetatable(self.view, {").AppendLine();
								builder.Append("\t\t\t\t").Append("__index = function(t, k)").AppendLine();
								builder.Append("\t\t\t\t\t").Append("local com = nil").AppendLine();
								for (int k = 0; k < view.properties.Count; k++)
								{
									var kv = view.properties[k];
									builder.Append("\t\t\t\t\t").AppendFormat("{0}if k == \"{1}\" then com = coms:Get({2}){3}", k > 0 ? "else" : "", kv.Key.ToLowerFirst(), k, (k == view.properties.Count - 1) ? " end" : "").AppendLine();
								}
								builder.Append("\t\t\t\t\t").Append("if com == nil then return mt.__index(self.view, k) end").AppendLine();
								builder.Append("\t\t\t\t\t").Append("rawset(t, k, com)").AppendLine();
								builder.Append("\t\t\t\t\t").Append("return com").AppendLine();
								builder.Append("\t\t\t\t").Append("end").AppendLine();
								builder.Append("\t\t\t").Append("})").AppendLine();
							}
						}
						m++;
					}
				}
				if (m > 0) {
					builder.Append("\t\t").AppendLine("end");
				}
				builder.Append("\t\t").AppendLine("self.mCachedViews[transform] = index");
				builder.Append("\t").AppendLine("end");
				builder.AppendLine("end");
				builder.AppendLine();
			}

			if (tableViews != null && tableViews.Count > 0) {
				// 通用的GetCellView方法
				builder.AppendFormat("function {0}:GetCellView(tableView, cell)", className).AppendLine();
				builder.Append("\t").AppendLine("local cellView = self.mCachedViews[cell]");
				builder.Append("\t").AppendLine("if cellView == nil then");
				builder.Append("\t\t").AppendLine("cellView = { }");
				int m = 0;
				for (int i = 0; i < tableViews.Count; i++) {
					var collection = tableViews[i];
					if (collection.views.Any((view) => view.properties.Count > 0)) {
						builder.Append("\t\t");
						if (m > 0) {
							builder.Append("else");
						}
						if (string.IsNullOrEmpty(collection.name)) {
							builder.AppendFormat("if tableView.name == \"{0}\" then", collection.objectName).AppendLine();
						} else {
							builder.AppendFormat("if tableView == self.view.{0} then", collection.name.ToLowerFirst()).AppendLine();
						}
						if (collection.views.Count > 1) {
							int n = 0;
							for (int j = 0; j < collection.views.Count; j++) {
								var cell = collection.views[j];
								if (cell.properties.Count > 0) {
									builder.Append("\t\t\t");
									if (n > 0) {
										builder.Append("else");
									}
									builder.AppendFormat("if cell.identifier == \"{0}\" then", cell.identifier).AppendLine();
									builder.Append("\t\t\t\t").Append("local coms = self.component:GetComponents(cell, false)").AppendLine();
									if (cell.properties.Count > 0)
									{
										builder.Append("\t\t\t\t").Append("setmetatable(cellView, {").AppendLine();
										builder.Append("\t\t\t\t\t").Append("__index = function(t, k)").AppendLine();
										builder.Append("\t\t\t\t\t\t").Append("local com = nil").AppendLine();
										for (int k = 0; k < cell.properties.Count; k++)
										{
											var kv = cell.properties[k];
											builder.Append("\t\t\t\t\t\t").AppendFormat("{0}if k == \"{1}\" then com = coms:Get({2}){3}", k > 0 ? "else" : "", kv.Key.ToLowerFirst(), k, (k == cell.properties.Count - 1) ? " end" : "").AppendLine();
										}
										builder.Append("\t\t\t\t\t\t").Append("rawset(t, k, com)").AppendLine();
										builder.Append("\t\t\t\t\t\t").Append("return com").AppendLine();
										builder.Append("\t\t\t\t\t").Append("end").AppendLine();
										builder.Append("\t\t\t\t").Append("})").AppendLine();
									}
									n++;
								}
							}
							if (n > 0) {
								builder.Append("\t\t\t").AppendLine("end");
							}
						} else {
							var cell = collection.views[0];
							builder.Append("\t\t\t").Append("local coms = self.component:GetComponents(cell, false)").AppendLine();
							if (cell.properties.Count > 0)
							{
								builder.Append("\t\t\t").Append("setmetatable(cellView, {").AppendLine();
								builder.Append("\t\t\t\t").Append("__index = function(t, k)").AppendLine();
								builder.Append("\t\t\t\t\t").Append("local com = nil").AppendLine();
								for (int k = 0; k < cell.properties.Count; k++)
								{
									var kv = cell.properties[k];
									builder.Append("\t\t\t\t\t").AppendFormat("{0}if k == \"{1}\" then com = coms:Get({2}){3}", k > 0 ? "else" : "", kv.Key.ToLowerFirst(), k, (k == cell.properties.Count - 1) ? " end" : "").AppendLine();
								}
								builder.Append("\t\t\t\t\t").Append("rawset(t, k, com)").AppendLine();
								builder.Append("\t\t\t\t\t").Append("return com").AppendLine();
								builder.Append("\t\t\t\t").Append("end").AppendLine();
								builder.Append("\t\t\t").Append("})").AppendLine();
							}
						}
						m++;
					}
				}
				if (m > 0) {
					builder.Append("\t\t").AppendLine("end");
				}
				builder.Append("\t\t").AppendLine("self.mCachedViews[cell] = cellView");
				builder.Append("\t").AppendLine("end");
				builder.Append("\t").AppendLine("return cellView");
				builder.AppendLine("end");
				builder.AppendLine();
			}

			builder.AppendFormat("return {0}", className);

			FileHelper.WriteString(file, builder.ToString());
		}
	}
}
