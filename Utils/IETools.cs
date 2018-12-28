using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEFParser.Utils
{
    class CEFTools
    {
        public static String querySelectorAll = @"(function(d,s){d=document,s=d.createStyleSheet();d.querySelectorAll=function(r,c,i,j,a){a=d.all,c=[],r=r.replace(/\[for\b/gi,'[htmlFor').split(',');for(i=r.length;i--;){s.addRule(r[i],'k:v');for(j=a.length;j--;)a[j].currentStyle.k&&c.push(a[j]);s.removeRule(0)}return c}})()";

        public static String querySelectorAll2 = @"
            if (!document.querySelectorAll) {
              document.querySelectorAll = function (selectors) {
                var style = document.createElement('style'), elements = [], element;
                document.documentElement.firstChild.appendChild(style);
                document._qsa = [];

                style.styleSheet.cssText = selectors + '{x-qsa:expression(document._qsa && document._qsa.push(this))}';
                window.scrollBy(0, 0);
                style.parentNode.removeChild(style);

                while (document._qsa.length) {
                  element = document._qsa.shift();
                  element.style.removeAttribute('x-qsa');
                  elements.push(element);
                }
                document._qsa = null;
                return elements;
              };
            }

            if (!document.querySelector) {
              document.querySelector = function (selectors) {
                var elements = document.querySelectorAll(selectors);
                return (elements.length) ? elements[0] : null;
              };
            }
        ";

        public static String createStyleSheet = @"
            if  (typeof document.createStyleSheet === 'undefined') {
              document.createStyleSheet = function () {
                return document.createElement('script');
              }
            }
        ";

        public static String isHidden = @"
            (function() { 
                window.isHidden = function (el) {
                    if ('getComputedStyle' in window && el != null) {
                        var style = window.getComputedStyle(el);
                        if (style == null) return false;
                        return ((style.display === 'none') || (style.visibility === 'hidden'))
                    } else { return false; }
                }
            })()
        ";
    }
}
