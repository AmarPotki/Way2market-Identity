(function(){

"use strict";

var app = angular.module('myPage', ['ngTouch']);
app.controller('myPageCtrl', function($scope,$http){








	/* ------------------------------ */
	/* EFFECTS
	/* ------------------------------ */

	$scope.setEffect = function() {
		$scope.html = $('html'),
		$scope.container = $('.container');
		$scope.rainyBlockInsert = function() {
			$scope.container.addClass('rainy_page')
				.append('<img id="rainy_background" alt="rainy" src="">');
			$scope.run = function() {
				$scope.image = document.getElementById('rainy_background');
				$scope.image.onload = function() {
					var engine = new RainyDay({
						image: this
					});
					engine.rain([ [1, 2, 4000] ]);
					engine.rain([ [3, 3, 0.88], [5, 5, 0.9], [6, 2, 1] ], 100);
				};
				$scope.image.crossOrigin = 'anonymous';
				$scope.image.src = 'images/photo_1.jpg';
			}
			$scope.run();
		}
		$scope.particlesBlockInsert = function() {
			$scope.container.append('<div id="particles_block"></div>');
			particlesJS('particles_block',
			  {
				"particles": {
				  "number": {
					"value": 80,
					"density": {
					  "enable": true,
					  "value_area": 800
					}
				  },
				  "color": {
					"value": "#ffffff"
				  },
				  "shape": {
					"type": "circle",
					"stroke": {
					  "width": 0,
					  "color": "#000000"
					},
					"polygon": {
					  "nb_sides": 5
					},
					"image": {
					  "src": "img/github.svg",
					  "width": 100,
					  "height": 100
					}
				  },
				  "opacity": {
					"value": 0.5,
					"random": false,
					"anim": {
					  "enable": false,
					  "speed": 1,
					  "opacity_min": 0.1,
					  "sync": false
					}
				  },
				  "size": {
					"value": 5,
					"random": true,
					"anim": {
					  "enable": false,
					  "speed": 40,
					  "size_min": 0.1,
					  "sync": false
					}
				  },
				  "line_linked": {
					"enable": true,
					"distance": 150,
					"color": "#ffffff",
					"opacity": 0.4,
					"width": 1
				  },
				  "move": {
					"enable": true,
					"speed": 6,
					"direction": "none",
					"random": false,
					"straight": false,
					"out_mode": "out",
					"attract": {
					  "enable": false,
					  "rotateX": 600,
					  "rotateY": 1200
					}
				  }
				},
				"interactivity": {
				  "detect_on": "canvas",
				  "events": {
					"onhover": {
					  "enable": true,
					  "mode": "repulse"
					},
					"onclick": {
					  "enable": true,
					  "mode": "push"
					},
					"resize": true
				  },
				  "modes": {
					"grab": {
					  "distance": 400,
					  "line_linked": {
						"opacity": 1
					  }
					},
					"bubble": {
					  "distance": 400,
					  "size": 40,
					  "duration": 2,
					  "opacity": 8,
					  "speed": 3
					},
					"repulse": {
					  "distance": 200
					},
					"push": {
					  "particles_nb": 4
					},
					"remove": {
					  "particles_nb": 2
					}
				  }
				},
				"retina_detect": true,
				"config_demo": {
				  "hide_card": false,
				  "background_color": "#555555",
				  "background_image": "",
				  "background_position": "50% 50%",
				  "background_repeat": "no-repeat",
				  "background_size": "cover"
				}
			  }

			)
		}
		$scope.winterBlockInsert = function() {
			$scope.container.append('<div id="winter_block"></div>');
			particlesJS('winter_block',
				{
					"particles": {
						"number": {
							"value": 400,
							"density": {
								"enable": true,
								"value_area": 800
							}
						},
						"color": {
							"value": "#fff"
						},
						"shape": {
							"type": "circle",
							"stroke": {
								"width": 0,
								"color": "#000000"
							},
							"polygon": {
								"nb_sides": 5
							},
							"image": {
								"src": "img/github.svg",
								"width": 100,
								"height": 100
							}
						},
						"opacity": {
							"value": 0.5,
							"random": true,
							"anim": {
								"enable": false,
								"speed": 1,
								"opacity_min": 0.1,
								"sync": false
							}
						},
						"size": {
							"value": 7,
							"random": true,
							"anim": {
								"enable": false,
								"speed": 40,
								"size_min": 0.1,
								"sync": false
							}
						},
						"line_linked": {
							"enable": false,
							"distance": 500,
							"color": "#ffffff",
							"opacity": 0.4,
							"width": 2
						},
						"move": {
							"enable": true,
							"speed": 6,
							"direction": "bottom",
							"random": false,
							"straight": false,
							"out_mode": "out",
							"bounce": false,
							"attract": {
								"enable": false,
								"rotateX": 600,
								"rotateY": 1200
							}
						}
					},
					"interactivity": {
						"detect_on": "canvas",
						"events": {
							"onhover": {
								"enable": true,
								"mode": "bubble"
							},
							"onclick": {
								"enable": true,
								"mode": "repulse"
							},
							"resize": true
						},
						"modes": {
							"grab": {
								"distance": 400,
								"line_linked": {
									"opacity": 0.5
								}
							},
							"bubble": {
								"distance": 400,
								"size": 4,
								"duration": 0.3,
								"opacity": 1,
								"speed": 3
							},
							"repulse": {
								"distance": 200,
								"duration": 0.4
							},
							"push": {
								"particles_nb": 4
							},
							"remove": {
								"particles_nb": 2
							}
						}
					},
					"retina_detect": true
				}

			)
		}
		if( !(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) ) {
			if ($scope.html.hasClass('water_effect')) {
				$scope.container.ripples({
					resolution: 512,
					dropRadius: 20,
					perturbance: 0.04
				});
			} else if ($scope.html.hasClass('particles_effect')) {
				$scope.particlesBlockInsert();
			} else if ($scope.html.hasClass('winter_effect')) {
				$scope.winterBlockInsert();
			} else if ($scope.html.hasClass('rainy_effect')) {
				$scope.rainyBlockInsert();
			}
		}

	}

	$scope.setEffect();

	


});

})();