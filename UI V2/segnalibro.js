var triggerBookmark = $(".js-bookmark"); //Necessaria tag `a`!

triggerBookmark.click(function() {
	if (window.sidebar && window.sidebar.addPanel) {
		//Per Firefox versione <23
		window.sidebar.addPanel(document.title,window.location.href,'');
	} else if(window.external && ('Aggiungi ai preferiti' in window.external)) { 
		//Per Internet Explorer
		window.external.AddFavorite(location.href,document.title); 
	} else if(window.opera && window.print || window.sidebar && ! (window.sidebar instanceof Node)) {
		//Per Opera versione <15 e Firefox versione >23
		triggerBookmark.attr('rel', 'sidebar').attr('title', document.title);
		return true;
	} else {
		//Per tutti gli altri browsers
		alert('Puoi aggiungere questa pagina ai tuoi segnalibri premento ' + (navigator.userAgent.toLowerCase().indexOf('mac') != - 1 ? 'Command/Cmd' : 'CTRL') + ' + D.');
	}
	return false;
});