using System.Collections.Generic;
using System;
using System.Linq;

namespace Pager
{
    public class PagedList<T> : List<T>
    {


        public int CurrentPage
        {
            get;
            set;
        }

        public List<T> CurrentPageList
        {
            get
            {
                return this.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            }
        }

        public int OffSet
        {
            get;
            set;
        }

        public int Next
        {
            get
            {
                if (this.CurrentPage == this.TotalPages) return this.CurrentPage;
                else return this.CurrentPage + 1;
            }
        }

        public int Previous
        {
            get
            {
                if (this.CurrentPage == 1) return this.CurrentPage;
                else return this.CurrentPage - 1;
            }
        }

        public int TotalPages
        {
            get;
            set;
        }

        public int TotalItems
        {
            get;
            set;
        }

        public int PageSize
        {
            get;
            set;
        }


        public PagedList(List<T> list, int pageSize, int currentpage)
        {
            PageSize = pageSize;
            TotalItems = list.Count();
            TotalPages = TotalItems / pageSize + (TotalItems % PageSize > 0 ? 1 : 0);
            CurrentPage = currentpage;
            this.AddRange(list);
        }

        public PagedList(List<T> list, int pageSize)
        {
            PageSize = pageSize;
            TotalItems = list.Count();
            TotalPages = TotalItems / pageSize + (TotalItems % PageSize > 0 ? 1 : 0);
            CurrentPage = 1;
            this.OffSet = 2;
            this.AddRange(list);
        }

        public List<T> GetPageList(int index)
        {
            if (index > 0)
                return this.Skip((index - 1) * this.PageSize).Take(this.PageSize).ToList();
            else return null;
        }

    }

}

