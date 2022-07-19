using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace DictionaryHandler {
    public static class OxfordLearnersDictionaries {
        public static string HideControls(WebBrowser webBrowser) {
            try {
                mshtml.HTMLDocument document = (mshtml.HTMLDocument)webBrowser.Document;
                var searchbar = document.getElementById("searchbar");
                if (searchbar != null) {
                    // Can't set none to important, so remove all other styling;
                    searchbar.className = "";
                    searchbar.style.display = "none";
                } else
                    return "First sadness\n";
                var smartphone_menu = document.getElementById("smartphone-menu");
                if (smartphone_menu != null) {
                    // Can't set none to important, so remove all other styling;
                    smartphone_menu.className = "";
                    smartphone_menu.style.display = "none";
                } else
                    return "Second sadness\n";
                var topslot_container = document.getElementById("topslot_container");
                if (topslot_container != null) {
                    // Can't set none to important, so remove all other styling;
                    topslot_container.className = "";
                    topslot_container.style.display = "none";
                } else
                    return "Last second\n";
                return "Patch sucsess\n";
            } catch (Exception ex) {
                return ex.ToString();
            }

        }
    }
}
