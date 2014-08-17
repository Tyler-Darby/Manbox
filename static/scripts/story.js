$.ajax({ url: 'http://berkin.me/manbox/lorem_ipsum', async: false }).done(function(xs) {
        console.log('OK');
    $('.example').text('"' + xs + '"');
});