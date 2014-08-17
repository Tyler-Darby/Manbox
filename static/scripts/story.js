$.ajax({ url: 'http://berkin.me/manbox/lorem_ipsum' }).done(function(xs) {
        console.log('OK');
    $('.example').text('"' + xs + '"');
});