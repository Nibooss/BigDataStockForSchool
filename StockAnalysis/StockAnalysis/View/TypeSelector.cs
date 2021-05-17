using StockAnalysis.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StockAnalysis.View
{
    public partial class TypeSelector : DataTemplateSelector, IList
    {
        Dictionary<Type, DataTemplate> internalDictionary = new Dictionary<Type, DataTemplate>();

        

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Precondiction check
            if (item == null)
            {
                return null;
            }

            // Determine target Type
            Type type = null;
            type ??= (item as Selectable)?.Value as Type;
            type ??= item as Type;
            type ??= item.GetType();


            DataTemplate retMe = null;
            if(internalDictionary?.TryGetValue(item.GetType(), out retMe) == true)
            {
                return retMe;
            }

            Type altType = internalDictionary.Keys.FirstOrDefault(t => t.IsAssignableFrom(type));

            if(altType != null)
            {
                return internalDictionary[altType];
            }

            return base.SelectTemplate(item, container);
        }

    }

    public partial class TypeSelector : DataTemplateSelector, IList
    {
        public object this[int index] 
        {
            get => internalDictionary.Values.ToList()[index];
            set => _ = 0; // TODO: Implement this
        }

        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => null;

        public void Add(DataTemplate item)
        {
            if (item.DataType is Type dataType)
            {
                internalDictionary.Add(dataType, item);
            }
        }

        public int Add(object value)
        {
            if(value is DataTemplate dt)
            { 
                if (dt.DataType is Type dataType)
                {
                    internalDictionary.Add(dataType, dt);
                }
            }
            return internalDictionary.Count() - 1;
        }

        public bool Contains(object value)
        {
            if(value is Type tp)
            {
                return internalDictionary.ContainsKey(tp);
            }
            if(value is DataTemplate dt)
            {
                return internalDictionary.ContainsKey(dt.DataType as Type) || internalDictionary.ContainsValue(dt);
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }



        public void Clear()
        {
            internalDictionary.Clear();
        }

        public bool Contains(DataTemplate item)
        {
            return internalDictionary.Values.Contains(item);
        }

        public bool Contains(Type type)
        {
            return internalDictionary.Keys.Contains(type);
        }

        public void CopyTo(DataTemplate[] array, int arrayIndex)
        {
            internalDictionary.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(DataTemplate item)
        {
            if(item.DataType is Type dataType)
            {
                return internalDictionary.Remove(dataType);
            }
            return false;
        }

        public bool Remove(Type type)
        {
            return internalDictionary.Remove(type);
        }

        public IEnumerator<DataTemplate> GetEnumerator()
        {
            return internalDictionary.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalDictionary.Values.GetEnumerator();
        }
    }
}
