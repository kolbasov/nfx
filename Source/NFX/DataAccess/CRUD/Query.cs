/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.DataAccess.CRUD
{
    /// <summary>
    /// Defines a query sent into ICRUDDataStore implementor to retrieve data.
    /// A Query is a named bag of paremeters where every parameter has a name and the value. 
    /// </summary>
    /// <remarks>
    ///  Keep in mind that a particular datastore implementation may have to deal with hybrid distributed backends where different tables get stored 
    ///  in different repositories (different architectures i.e. NoSQL, HDFS, and some RDB SQL all in different locations),
    ///  consequently one can not make assumption about writing SQLs of any kind in business code - that is what CRUD concept is for as it abstracts 
    ///  this into provider implementation.
    ///  Architectural note: unlike Hibernate, LinqTo* and the like, the NFX.CRUD architecture purposely does not allow developers 
    ///  to write query scripts in higher-language like C#. This is because translation of such a high-level language abstraction into
    ///   highly optimized SQL/(and or other script) per particular backend is impossible because such language can not incapsulate
    ///  the optimization features of all possible data backends (i.e. ORACLE vs MongoDB vs Redis vs Files in HDFS).
    /// CRUD queries need to support selects from tables with millions of rows, or reads from collections with millions of documents,
    /// or parse millions of lines from files stored in Hadooop servers, thus every particular provider for every particular business app
    /// must expose custom-written queries by name. Those queries are usually highly optimized for particular platform 
    /// (i.e. using db-specific hints, common table subexpressions, groupping sets etc.).
    /// Also, a provider may elect to SELECT * from a table named like Query object, when a hand-written script with such name is not found
    /// </remarks>
    [Serializable]
    public sealed class Query : List<Query.Param>, IParameters, INamed
    {
            #region Inner Classes
                /// <summary>
                /// Represents a CRUD query parameter
                /// </summary>
                [Serializable]
                public sealed class Param : IParameter
                {
                    public Param(string name, object value = null)
                    {
                        m_Name = name;
                        m_Value = value;
                    }

                    #region Fields
                        private string m_Name;  
                        private object m_Value; 
                    #endregion

                    #region Properties
                        public string Name   { get { return m_Name; } }

                        public object Value  { get { return m_Value;} }

                        public bool HasValue { get {return true; } }

                        public bool IsInput  { get { return true;} }
                    #endregion
                }
            #endregion
        
        #region .ctor
            public Query(string name, Type resultRowType = null, Dictionary<string, object> extra = null)
            {
                m_Name = name;
                m_Extra = extra;
                m_ResultRowType = resultRowType;
                checkResultRowType();
            }

            public Query(string name, IDataStoreKey key, Type resultRowType = null)
            {
                m_Name = name;
                m_DataStoreKey = key;
                m_ResultRowType = resultRowType;
                checkResultRowType();
            }

        #endregion
        
        #region Fields
            
            private string m_Name;
            private Dictionary<string, object> m_Extra;
            private IDataStoreKey m_DataStoreKey;
            private Type m_ResultRowType;

        #endregion


        #region Properties
            
            /// <summary>
            /// Returns Query name, providers use it to locate SQL/scripts particular to backend implementation that they represent.
            /// QueryResolver resolves query by its name into ICRUDQueryHandler. Name is case-insensitive
            /// </summary>
            public string Name
            {
                get { return m_Name;}
            }


            /// <summary>
            /// Returns the key if one was passed in .ctor when key is set the parameters are ignored
            /// </summary>
            public IDataStoreKey StoreKey
            {
                get {return m_DataStoreKey;}
            }


            /// <summary>
            /// Returns a type of result row requested in .ctor which is always a TypedRow derivative type, or null
            ///  if no particular type was requested
            /// </summary>
            public Type ResultRowType
            {
                get { return m_ResultRowType; }
            }


            /// <summary>
            /// Returns extra parameters that provider may need to render the query. May be null
            /// </summary>
            public Dictionary<string, object>  Extra
            {
                get { return m_Extra;}
            }


            /// <summary>
            /// Returns parameter by its name or null
            /// </summary>
            public Param this[string name]
            {
                get {return FindParamByName(name) as Param;}
            }
            
            
            public IEnumerable<IParameter> AllParameters
            {
                get { return this; }
            }

            public IParameter ParamByName(string name)
            {
                return this.First(p => name.EqualsIgnoreCase(p.Name));
            }

            public IParameter FindParamByName(string name)
            {
                return this.FirstOrDefault(p => name.EqualsIgnoreCase(p.Name));
            }
       #endregion

       #region Public

            public override string ToString()
            {
                return "Query[{0}]".Args(Name);
            }

       #endregion

       #region .pvt
            
            private void checkResultRowType()
            {
                if (m_ResultRowType==null) return;
                if (!typeof(Row).IsAssignableFrom(m_ResultRowType))
                    throw new CRUDException(StringConsts.CRUD_TYPE_IS_NOT_DERIVED_FROM_ROW_ERROR.Args(m_ResultRowType.FullName));
            }
        
       #endregion
    }

   


}
