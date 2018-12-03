using csharpHelp.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.services {

	public class XmlModelServer{
		//public string rootXmlName = "";
		public XmlCtl _xml { get; set; } = null;
		public object data { get; set; } = null;

		//Dictionary<string, Func<string, object>> mapConvert = new Dictionary<string, Func<string, object>>();

		public XmlModelServer() {

		}

		public XmlModelServer(object _data, XmlCtl __xml = null) {
			data = _data;
			_xml = __xml;
		}

		public XmlCtl xml {
			get { return _xml; }
			set { _xml = value; }
		}

		//private void addConvert<T>(Func<string, object> fun) {
		//	mapConvert[typeof(T).ToString()] = fun;
		//}

		//private void addConvert<TType>(Func<string, TType> fun) {
		//	mapConvert[typeof(TType).ToString()] = (string str) => fun(str);
		//}

		/// <summary></summary>
		/// <param name="xml"></param>
		public void loadFromXml() {
			_load(data, "");
		}

		private void _load(object subData, string rootPath) {
			Type type = subData.GetType();
			
			if(subData == data && type.IsDefined(typeof(XmlRoot))) {
				string path = type.GetCustomAttribute<XmlRoot>().path;
				rootPath = path + ".";
			}

			var arrFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			var arrProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			foreach(var mi in arr) {
				object val = getMemberValue(mi, subData);
				//Debug.WriteLine(pi.GetType() + "," + pi.Name);

				if(mi.IsDefined(typeof(XmlAttr), false)) {
					//attr
					string path = rootPath + mi.GetCustomAttribute<XmlAttr>().path;
					setAttr(subData, mi, path);
				} else if(mi.IsDefined(typeof(XmlValue), false)) {
					//value
					string path = rootPath + mi.GetCustomAttribute<XmlValue>().path;
					setData(subData, mi, _xml.value, path);
				}else if(mi.IsDefined(typeof(XmlChild), false)) {
					//object
					string path = rootPath + mi.GetCustomAttribute<XmlChild>().path;
					
					if(val.GetType().IsClass) {
						_load(val, path + ".");
					}
				}
			}
		}

		private void setMemberValue(MemberInfo mi, object data, object value) {
			if(mi.MemberType == MemberTypes.Field) {
				(mi as FieldInfo).SetValue(data, value);
			} else if(mi.MemberType == MemberTypes.Property) {
				(mi as PropertyInfo).SetValue(data, value);
			}
		}

		private object getMemberValue(MemberInfo mi, object data) {
			object rst = null;
			if(mi.MemberType == MemberTypes.Field) {
				rst = (mi as FieldInfo).GetValue(data);
			} else if(mi.MemberType == MemberTypes.Property) {
				rst = (mi as PropertyInfo).GetValue(data);
			} else {
				return null;
			}

			return rst;
		}

		private Type getMemberType(MemberInfo mi) {
			Type rst = null;
			if(mi.MemberType == MemberTypes.Field) {
				rst = (mi as FieldInfo).FieldType;
			} else if(mi.MemberType == MemberTypes.Property) {
				rst = (mi as PropertyInfo).PropertyType;
			} else {
				return null;
			}

			return rst;
		}

		private void setAttr(object data, MemberInfo mi, string path) {
			object val = getMemberValue(mi, data);

			Type type = getMemberType(mi);
			if(type.IsGenericType) {
				//泛型
				Type[] arrType = type.GetGenericArguments();
				if(arrType.Length == 1) {
					//IList
					if(arrType[0].IsClass) {
						//Debug.WriteLine("aaa:" + arrType[0]);
						return;
					} else {
						//object obj = Activator.CreateInstance(arrType[0]);

						int idx = path.LastIndexOf(".");
						string rootPath = "";
						string attrName = path;
						string subPath = path;
						if(idx >= 0) {
							rootPath = path.Substring(0, idx);
							attrName = path.Substring(idx + 1);

							subPath = rootPath;
							idx = rootPath.LastIndexOf(".");
							if(idx >= 0) {
								subPath = rootPath.Substring(idx + 1);
								rootPath = rootPath.Substring(0, idx);
							}
						}

						object lst = val;
						if(lst == null) {
							lst = Activator.CreateInstance(type);
							setMemberValue(mi, data, lst);
						}

						//IList<int> aa = new List<int>();
						//aa.Clear();
						//aa.Add(3);

						var mtChear = type.GetMethod("Clear");
						mtChear.Invoke(lst, new object[0]);

						var mtAdd = type.GetMethod("Add", new Type[] { arrType[0] });

						//Debug.WriteLine("aa:" + rootPath + "," + subPath + "," + attrName);
						_xml.each(rootPath + "." + subPath, (idx2, xml) => {
							var strValue = xml.attr(attrName);
							//Debug.WriteLine("111:" + xml.name() + "," + xml.attr(attrName));
							object obj = Convert.ChangeType(strValue, arrType[0]);
							//Debug.WriteLine("aa:" + obj.GetType());

							mtAdd.Invoke(lst, new object[] { obj });
						});

						//var strValue = getDataFun(path, val.ToString());
						//object rst = Convert.ChangeType(strValue, val.GetType());
						//Debug.WriteLine("bbb");

						return;
					}
				} else {
					return;
				}
				//Debug.WriteLine("aa:" + type.GetGenericArguments() + "," + type.GetGenericTypeDefinition());
			} else {
				if(val == null) {
					return;
				}

				var strValue = xml.attr(path, val.ToString());

				string strType = val.GetType().ToString();
				object rst = Convert.ChangeType(strValue, val.GetType());
				setMemberValue(mi, data, rst);
			}
		}

		private void setData(object data, MemberInfo pi, Func<string, string, string> getDataFun, string path) {
			object val = getMemberValue(pi, data);

			Type type = val.GetType();
			if (type.IsGenericType) {
				Type[] arrType = type.GetGenericArguments();
				if (arrType.Length == 1) {
					if (arrType[0].IsClass) {
						Debug.WriteLine("aaa:" + arrType[0]);
						return;
					} else {
						object obj = Activator.CreateInstance(arrType[0]);

						int idx = path.LastIndexOf(".");
						string rootPath = "";
						string subPath = path;
						if (idx >= 0) {
							rootPath = path.Substring(0, idx);
							subPath = path.Substring(idx + 1);
						}

						Debug.WriteLine("aa:" + rootPath + "," + subPath);
						_xml.each(rootPath, (idx2, xml) => {
							Debug.WriteLine("111:" + xml.name());
						});

						//var strValue = getDataFun(path, val.ToString());
						//object rst = Convert.ChangeType(strValue, val.GetType());
						//Debug.WriteLine("bbb");

						return;
					}
				}
				//Debug.WriteLine("aa:" + type.GetGenericArguments() + "," + type.GetGenericTypeDefinition());
			}

			var strValue = getDataFun(path, val.ToString());

			string strType = val.GetType().ToString();
			object rst = Convert.ChangeType(strValue, val.GetType());
			setMemberValue(pi, data, rst);
		}

		public void saveToXml() {
			_save(data, "");
		}

		private void _save(object subData, string rootPath) {
			Type type = subData.GetType();

			var arrFields = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
			var arrProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			MemberInfo[] arr = arrFields.Cast<MemberInfo>().Concat(arrProps).ToArray();

			//string rootPath = "";
			if(subData == data && type.IsDefined(typeof(XmlRoot))) {
				//var attr = type.GetCustomAttribute<XmlPath>();
				string path = type.GetCustomAttribute<XmlRoot>().path;
				rootPath = path + ".";
			}

			foreach(var pi in arr) {
				//string name = pi.Name;
				object val = getMemberValue(pi, subData);
				//object val = pi.GetValue(subData);
				//Debug.WriteLine("bb:" + name + "," + val);

				if(pi.IsDefined(typeof(XmlAttr), false)) {
					string path = rootPath + pi.GetCustomAttribute<XmlAttr>().path;
					_xml.setAttr(path, val.ToString());
				} else if(pi.IsDefined(typeof(XmlValue), false)) {
					string path = rootPath + pi.GetCustomAttribute<XmlValue>().path;
					_xml.setValue(path, val.ToString());
				} else if(pi.IsDefined(typeof(XmlChild), false)) {
					string path = rootPath + pi.GetCustomAttribute<XmlChild>().path;
					if(val.GetType().IsClass) {
						_save(val, path + ".");
					}
				}
			}
		}

	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlAttr : Attribute {
		public string path = "";

		public XmlAttr(string _path) {
			path = _path;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlValue : Attribute {
		public string path = "";

		public XmlValue(string _path) {
			path = _path;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class XmlRoot : Attribute {
		public string path = "";

		public XmlRoot(string _path = "") {
			path = _path;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class XmlChild : Attribute {
		public string path = "";

		public XmlChild(string _path = "") {
			path = _path;
		}
	}

	//public class XmlUnknownTypeException : Exception {
	//	public Type unkownType = null;

	//	public XmlUnknownTypeException() { }

	//	public XmlUnknownTypeException(Type _type) : base("unkown type " + _type.ToString()) {
	//		unkownType = _type;
	//	}
	//}
}
